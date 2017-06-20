# Microsoft Automatic Graph Layout for UWP
**MsAGL** is a .NET UWP library and tool for graph layout and viewing. 

MsAGL was developed in Microsoft by Lev Nachmanson, Sergey Pupyrev, Tim Dwyer, Ted Hart, and Roman Prutkin.

## Getting Started

The simplest way to start with MSAGL in C# is to clone or download this repository and the open MsAGL.sln in Visual Studio, and and look at the SimpleApp in the Samples folder.

## MSAGL Modules

**The Core Layout engine (Microsoft.MSAGL.dll)** 
This .NET asssembly contains the core layout functionality. Use this library if you just want MSAGL to perform the layout only and afterwards you will use a separate tool to perform the rendering and visalization.

**The Drawing module (Microsoft.MSAGL.Drawing.dll)** 
The Definitions of different drawing attributes like colors, line styles, etc. It also contains definitions of a node class, an edge class, and a graph class. By using these classes a user can create a graph object and use it later for layout, and rendering.

**A Viewer control** (Microsoft.MSAGL.GraphControl.dll)** 
The viewer control lets you visualize graphs and has and some other rendering functionality. Key features: (1) Pan and Zoom (2) Navigate Forward and Backward (3) tooltips and highlighting on graph entities (4) Search for and focus on graph entities.