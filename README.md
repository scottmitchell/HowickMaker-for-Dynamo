# HowickMaker

HowickMaker for Dynamo is a set of tools for programming steel stud roll-forming machines with Dynamo. Currently, the plugin supports fabrication with the Howick FRAMA 3200 machine.

## Nodes
![picture alt](images/hMember.png?raw=true "hMember")
### hMember

Nodes for creation and manipulation of individual steel studs.

- **hMember.ByLineVector** : Creates a steed stud (hMember) with the provided web axis (the Line running down the center of the stud's web) and web normal (a Vector pointing from the web of the stud to the lip of the stud).
- **hMember.AddOperationByLocationType** : Adds an operation to the stud at a specified location along the len
- **hMember.AddOperationByPointType** : Adds an operation to the stud at a specified point. 
- **hMember.WebAxis** : Returns the web axis of the stud.
- **hMember.FlangeAxes** : Returns the flange axes of the stud.
- **hMember.AsCSVLine** : Returns the stud as a string for fabrication.
- **hMember.ExportToFile** : Exports a list of hMembers to file for fabrication.

### hStructure

Nodes for solving large collections of steel studs. 

- **hStructure.FromLines** : Returns a list of hMembers with web axes defined by the input lines. Web normals and connection types for the hMembers are determined by the solver (assuming you gave it valid lines!). Returned members contain all operations neccesary for their associated connections.
- **hStructure.StructureOptions** : Optional parameters for creating an hStructure.
    - Intersection Tolerance (double) : The tolerance used when determining whether two lines intersect. Intersecting lines become connected studs. 
    - Planarity Tolerance (double) : The tolerance used when determining whether connections between members are valid. Perfectly planar connections are ideal, but studs can be twisted and bent to accomodate small planar inconsistencies.
    - Three Piece Brace (boolean) : Default braces are a single member. However, with large-angled braced connections ( > ~150°), it becomes neccesary to fix the brace with three members.
    - Brace Length 1 (double) : The length of the primary brace(s) (the only brace if threePieceBrace == false, the twin braces if threePieceBrace == true).
    - Brace Length 2 (double) : The length of the secondary brace.
    - First Connection is Face-To-Face (boolean) : The first hMember is the first line in your list of input lines. The first hMember's first connection is the first line in the list that it's web axis intersects (other than itself). Should this connection be face-to-face or braced? 

### hLines / hMesh

hLines and hMesh contain nodes for particular workflows, and particular strategies of parametric steel stud fabrication and construction. Currently, this includes several quad-mesh and triangular-mesh strategies. More will be added as they are developed.