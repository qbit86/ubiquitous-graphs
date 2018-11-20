﻿// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Ubiquitous.Traversal.Advanced
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Internal;
    using static System.Diagnostics.Debug;

    public partial struct DfsForestStepCollection<TGraph, TVertex, TEdge,
        TVertexEnumerable, TVertexEnumerator, TEdgeEnumerator,
        TColorMap, TGraphPolicy, TColorMapPolicy, TVertexEnumerablePolicy>
    {
        public struct Enumerator : IEnumerator<Step<DfsStepKind, TVertex, TEdge>>
        {
            private Step<DfsStepKind, TVertex, TEdge> _current;
            private int _state;

            private TColorMapPolicy _colorMapPolicy;
            private TGraphPolicy _graphPolicy;

            private readonly TGraph _graph;

            // See explanations why fields are not readonly here:
            // https://codeblog.jonskeet.uk/2014/07/16/micro-optimization-the-surprising-inefficiency-of-readonly-fields/
            private TVertexEnumerator _vertexEnumerator;
            private readonly int _stackCapacity;
            private TColorMap _colorMap;
            private DisposalStatus _colorMapDisposalStatus;
            private List<DfsStackFrame<TVertex, TEdge, TEdgeEnumerator>> _stack;
            private DisposalStatus _stackDisposalStatus;

            private DfsStepEnumerator<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TGraphPolicy,
                TColorMapPolicy> _stepEnumerator;

            internal Enumerator(DfsForestStepCollection<TGraph, TVertex, TEdge,
                TVertexEnumerable, TVertexEnumerator, TEdgeEnumerator,
                TColorMap, TGraphPolicy, TColorMapPolicy, TVertexEnumerablePolicy> collection)
            {
                Assert(collection.ColorMapPolicy != null);

                _current = default(Step<DfsStepKind, TVertex, TEdge>);
                _state = 0;

                _graph = collection.Graph;
                _vertexEnumerator = collection.VertexEnumerablePolicy.GetEnumerator(collection.VertexCollection);
                _stackCapacity = collection.StackCapacity;
                _graphPolicy = collection.GraphPolicy;
                _colorMapPolicy = collection.ColorMapPolicy;
                _colorMap = default(TColorMap);
                _colorMapDisposalStatus = DisposalStatus.None;
                _stack = null;
                _stackDisposalStatus = DisposalStatus.None;
                _stepEnumerator =
                    default(DfsStepEnumerator<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TGraphPolicy,
                        TColorMapPolicy>);
            }

            public bool MoveNext()
            {
                while (true)
                {
                    switch (_state)
                    {
                        case 0:
                        {
                            _colorMap = _colorMapPolicy.Acquire();
                            if (_colorMap == null)
                            {
                                _state = int.MaxValue;
                                continue;
                            }

                            _colorMapDisposalStatus = DisposalStatus.Initialized;
                            _state = 1;
                            continue;
                        }
                        case 1:
                        {
                            if (!_vertexEnumerator.MoveNext())
                            {
                                _state = int.MaxValue;
                                continue;
                            }

                            _state = 2;
                            continue;
                        }
                        case 2:
                        {
                            Assert(_colorMap != null, nameof(_colorMap) + " != null");
                            if (!_colorMapPolicy.TryGet(_colorMap, _vertexEnumerator.Current, out Color vertexColor))
                                vertexColor = Color.None;
                            if (vertexColor != Color.None && vertexColor != Color.White)
                            {
                                _state = 1;
                                continue;
                            }

                            _current = Step.Create(DfsStepKind.StartVertex, _vertexEnumerator.Current,
                                default(TEdge));
                            _state = 3;
                            return true;
                        }
                        case 3:
                        {
                            _stack =
                                ListCache<DfsStackFrame<TVertex, TEdge, TEdgeEnumerator>>.Acquire(_stackCapacity);
                            if (_stack == null)
                            {
                                _state = int.MaxValue;
                                continue;
                            }

                            _stackDisposalStatus = DisposalStatus.Initialized;
                            _state = 4;
                            continue;
                        }
                        case 4:
                        {
                            ThrowIfDisposed();
                            _stepEnumerator = new DfsStepEnumerator<TGraph, TVertex, TEdge, TEdgeEnumerator,
                                TColorMap, TGraphPolicy, TColorMapPolicy>(
                                _graph, _vertexEnumerator.Current, _colorMap, _stack, _graphPolicy, _colorMapPolicy);
                            _state = 5;
                            continue;
                        }
                        case 5:
                        {
                            ThrowIfDisposed();
                            if (!_stepEnumerator.MoveNext())
                            {
                                DisposeStack();
                                _state = 1;
                                continue;
                            }

                            _current = _stepEnumerator.Current;
                            _state = 5;
                            return true;
                        }
                        case int.MaxValue:
                        {
                            DisposeCore();
                            _current = default(Step<DfsStepKind, TVertex, TEdge>);
                            _state = -1;
                            return false;
                        }
                    }

                    return false;
                }
            }

            public void Reset()
            {
                DisposeCore();

                _current = default(Step<DfsStepKind, TVertex, TEdge>);
                _state = 0;

                _colorMap = default(TColorMap);
                _colorMapDisposalStatus = DisposalStatus.None;
                _stack = null;
                _stackDisposalStatus = DisposalStatus.None;
                _stepEnumerator =
                    default(DfsStepEnumerator<TGraph, TVertex, TEdge, TEdgeEnumerator, TColorMap, TGraphPolicy,
                        TColorMapPolicy>);
            }

            // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
            public Step<DfsStepKind, TVertex, TEdge> Current => _current;

            object IEnumerator.Current => _current;

            public void Dispose()
            {
                DisposeCore();
                _current = default(Step<DfsStepKind, TVertex, TEdge>);
                _state = -1;
            }

            private void DisposeStack()
            {
                if (_stackDisposalStatus == DisposalStatus.Initialized)
                {
                    ListCache<DfsStackFrame<TVertex, TEdge, TEdgeEnumerator>>.Release(_stack);
                    _stack = null;
                    _stackDisposalStatus = DisposalStatus.Disposed;
                }
            }

            private void DisposeCore()
            {
                if (_colorMapDisposalStatus == DisposalStatus.Initialized)
                {
                    _colorMapPolicy.Release(_colorMap);
                    _colorMap = default(TColorMap);
                    _colorMapDisposalStatus = DisposalStatus.Disposed;
                }

                DisposeStack();
            }

            private void ThrowIfDisposed()
            {
                if (_colorMapDisposalStatus != DisposalStatus.Initialized)
                    throw new ObjectDisposedException(nameof(_colorMap));
                if (_stackDisposalStatus != DisposalStatus.Initialized)
                    throw new ObjectDisposedException(nameof(_stack));
            }
        }
    }
}