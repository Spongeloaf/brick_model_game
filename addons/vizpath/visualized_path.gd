## Ths class is designed to create a mesh that
## will display a path between pairs of VisualizationSpots.
## 
## To use a VisualizedPath it must be populated with an array
## of spots which define the local object space positions and
## normals for the path at that point.
##
## The spots array can be defined programmatically such as by 
## user action (such as clicking) or defined in the editor using
## the VizPath gizmo.
##
## Not all paths are valid.  Putting the spots too close together 
## or with normals that do not leave enough room for bends to be
## created with the defined path characteristics (width, inner radius,
## etc.), will result in the path not being displayed and the "error"
## attribute being set to the underlying problem.
##

@tool
@icon("res://addons/vizpath/images/path.png")
extends Node3D
class_name VisualizedPath

signal changed_layout

# C# Interface Functions
func DrawPathWithHead(points : Array[Vector3]):
	if points == null:
		return
	if points.size() == 0:
		spots = []
		return
	
	var newSpots : Array[VisualizationSpot]
	for index in range(0, points.size() -1):
		var spot = VisualizationSpot.new()
		spot.point = points[index]
		
		# This is a problem. We need to concot a better way to calculate normals
		# so these render correctly. Right now, the normals don't work for short
		# line segments at steep angles. 
		spot.normal = Vector3.UP
		newSpots.append(spot)
	spots = newSpots


func PrintDebugInfo():
	print("---- Path Vis debug ----")
	for spot in spots:
		var formatted_str = "spot: %v, normal: %v"
		formatted_str = formatted_str % [spot.point, spot.normal]
		print(formatted_str)
	
	print("")
	print("Path props:")

	print("path_width")
	print(path_width)
	print("inner_curve_radius")
	print(inner_curve_radius)
	print("num_curve_segs")
	print(num_curve_segs)
	print("bend_segs")
	print(bend_segs)
	print("bend_lip")
	print(bend_lip)
	print("bend_sharpness")
	print(bend_sharpness)

	print("---- Path Vis debug fin ----")

## The spots array defines where the path will go between.
## See [VisualizationSpot]
@export var spots : Array[VisualizationSpot] :
	set(p_spots):
		spots = p_spots
		for spot in spots:
			if not spot.changed.is_connected(_rebuild):
				spot.changed.connect(_rebuild)
		_rebuild()

## The path_width defines the width of the path, which indirectly
## determines how tight the path can turn at a midpoint spot.
@export_range(0.005, 100, 0.001) var path_width := 0.1 :
	set(w):
		path_width = w
		_rebuild()

## The inner_curve_radius defines how tight a turn can be made
## at a midpoint spot within the path.
@export_range(0.005, 100, 0.001) var inner_curve_radius := 0.1 :
	set(r):
		inner_curve_radius = r
		_rebuild()

## The num_curve_segs defines how many points will be defined in the
## arc that are used to create a turn.  More segments will
## make a smoother UV mapping and therefore the display of associated material
## more accurate.
@export_range(4, 128) var num_curve_segs := 32 :
	set(r):
		num_curve_segs = r
		_rebuild()

## The bend_segs defines the segments in a bend of the path when transitioning
## between two spots with normals not in the same plane.
@export_range(4, 128) var bend_segs := 6 :
	set(r):
		bend_segs = r
		_rebuild()

## The bend_lip defines the distance that a segment extends from the intersection
## of the planes defined by the beginning and ending normals when those normals 
## are not in the same plane.  Making this a smaller value will make the 
## segment flatter for more of its length.
@export_range(0.005, 100, 0.001) var bend_lip := 0.1 :
	set(r):
		bend_lip = r
		_rebuild()

## The bend_sharpness defines how sharp the bend in a path.  Making this
## smaller will result in a tighter bend.
@export var bend_sharpness := 1.0 :
	set(r):
		bend_sharpness = r
		_rebuild()

## The path_mat is the material that will be applied to the underlying mesh
## that displays the path.  The initial spot in the path will have texture coordinates
## that start with U equal to 0.0 and ending at the value calculated as the actual length of the path
## in local space.  The V will range between 0.0 at the left side of the path, and 1.0 at the 
## right side of the path.
@export var path_mat : Material :
	set(m):
		path_mat = m
		_rebuild()

## The path_head is a resource (optional) that defines the end of the path by 
## providing an "apply" method.  The provided [VizHead] resource is an example
## that draws an arrow head on the end.
@export var path_head : VizHead :
	set(m):
		path_head = m
		path_head.changed.connect(_rebuild)
		_rebuild()

## The path_tail is a resource (optional) that defines the start of the path by 
## providing an "apply" method.  The provided [VizTail] resource is an example
## that draws a rounded cap on the end.
@export var path_tail : VizTail :
	set(m):
		path_tail = m
		path_tail.changed.connect(_rebuild)
		_rebuild()

@export var suppress_warnings := false :
	set(s):
		suppress_warnings = s
		_rebuild()

var _mesh_instance : MeshInstance3D
var _errors : Array[String]

func get_triangle_mesh() -> TriangleMesh:
	if _mesh_instance.mesh != null:
		return _mesh_instance.mesh.generate_triangle_mesh()
	return null

func _ready():
	_mesh_instance = MeshInstance3D.new()
	add_child(_mesh_instance)
	_rebuild()

func _get_configuration_warnings():
	if not suppress_warnings:
		return PackedStringArray(_errors)
	return PackedStringArray()

func get_errors() -> Array[String]:
	return _errors

func _rebuild():
	_errors = []
	if _mesh_instance == null:
		return
	if spots.size() < 2:
		_errors.append("the spots array must have at least 2 entries")
	_mesh_instance.mesh = null
	var u := 0.0
	var segments : Array[VizSegment] = []
	for idx in range(1, spots.size()):
		segments.push_back(VizSegment.new(spots[idx-1], spots[idx], path_width, bend_lip))

	for idx in range(1, segments.size()):
		if segments[idx-1].is_invalid():
			_errors.push_back(segments[idx-1].get_error())
			break
		var midpoint := VizMid.new(segments[idx-1], segments[idx], path_width, inner_curve_radius)
		if midpoint.is_invalid():
			_errors.push_back(midpoint.get_error())
			break
		# Creating midpoint may invalidate prior segment
		if segments[idx-1].is_invalid():
			_errors.push_back(segments[idx-1].get_error())
			break
		if idx == 1:
			u = _add_tail(segments[idx-1], u)
		u = segments[idx-1].update_mesh(_mesh_instance, u, bend_segs, bend_sharpness, path_mat)
		u = midpoint.update_mesh(_mesh_instance, u, num_curve_segs, path_mat)
	
	if _errors.size() == 0:
		if segments[segments.size()-1].is_invalid():
			_errors.push_back(segments[segments.size()-1].get_error())
		else:
			if segments.size() == 1:
				u = _add_tail(segments[0], u)
			_add_head(segments[segments.size()-1], u)
	else:
		pass
	
	update_configuration_warnings()
	changed_layout.emit()

func _add_head(head : VizSegment, u : float):
	if path_head != null:
		var end := head.get_end().point
		var binormal := head.get_end_binormal()
		var left := end - binormal * path_width / 2.0
		var right := end + binormal * path_width / 2.0
		var normal := head.get_end().normal
		var direction := head.get_end_ray()
		var offset := path_head.get_offset(left, right, normal, direction)
		head.adjust_end(offset)
		u = head.update_mesh(_mesh_instance, u, bend_segs, bend_sharpness, path_mat)
		path_head.apply(_mesh_instance, u, left - direction * offset, right - direction * offset, normal, direction, path_mat)
	else:
		head.update_mesh(_mesh_instance, u, bend_segs, bend_sharpness, path_mat)

func _add_tail(tail : VizSegment, u : float) -> float:
	if path_tail != null:
		var begin := tail.get_begin().point
		var binormal := tail.get_begin_binormal()
		var left := begin - binormal * path_width / 2.0
		var right := begin + binormal * path_width / 2.0
		var normal := tail.get_begin().normal
		var direction := tail.get_begin_ray()
		var offset := path_tail.get_offset(left, right, normal, direction)
		tail.adjust_begin(offset)
		path_tail.apply(_mesh_instance, u, left - direction * offset, right - direction * offset, normal, direction, path_mat)
		return u + offset
	return u
