﻿namespace Ubiquitous.Traversal
{
    public delegate void VertexEventHandler<in TGraph, in TVertex>(TGraph g, TVertex v);
}
