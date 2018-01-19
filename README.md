unity-subdivision-surface
=====================

Loop subdivision surface algorithm implementation in Unity.

![Heads](https://raw.githubusercontent.com/mattatz/unity-subdivision-surface/master/Captures/Heads.png)

From left to right, original mesh, subdivided 1 time, subdivided 2times model.

![Cubes](https://raw.githubusercontent.com/mattatz/unity-subdivision-surface/master/Captures/Cubes.png)

## Loop subdivision surface

> Loop subdivision surface is an approximating subdivision scheme developed by Charles Loop in 1987 for triangular meshes.
> [wikipedia](https://en.wikipedia.org/wiki/Loop_subdivision_surface)

## SubdivMorph

![SubdivMorph](https://raw.githubusercontent.com/mattatz/unity-subdivision-surface/master/Captures/SubdivMorph.gif)

SubdivMorph demo morph vertices between original and subdivided.

## Usage

```cs

var filter = GetComponent<MeshFilter>();

// Require a mesh to weld (require to remove duplicated vertices)
var welded = SubdivisionSurface.Weld(filter.mesh, float.Epsilon, filter.mesh.bounds.size.x);

var mesh = SubdivisionSurface.Subdivide(
  welded,   // a welded mesh
  2,        // subdivision count
  false     // a result mesh is welded or not
);
filter.sharedMesh = mesh;

```

See demo scenes for details.

## Compatibility

tested on Unity 2017.0.3, windows10 (GTX 1060).

## Sources

- Free low poly head model by hexonian - https://sketchfab.com/models/988a1ffdb6244eaab9b293d296c6e868

- Carnegie Mellon Univ lecture slide - http://www.cs.cmu.edu/afs/cs/academic/class/15462-s14/www/lec_slides/Subdivision.pdf

- Michigan Tech Univ lecture slide - https://pages.mtu.edu/~shene/COURSES/cs3621/SLIDES/Subdivision.pdf

- Matt's Webcorner - Subdivision - https://graphics.stanford.edu/~mdfisher/subdivision.html
