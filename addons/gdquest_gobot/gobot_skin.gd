extends Node3D
## Gobot's MeshInstance3D model.
@export var gobot_model: MeshInstance3D

@onready var _animation_tree: AnimationTree = %AnimationTree
@onready var _state_machine: AnimationNodeStateMachinePlayback = _animation_tree.get(
	"parameters/StateMachine/playback",
)

@onready var _flip_shot_path: String = "parameters/FlipShot/request"

## Sets the model to a neutral, action-free state.
func idle() -> void:
	_state_machine.travel("Idle")


## Sets the model to a running animation or forward movement.
func run() -> void:
	_state_machine.travel("Run")


## Sets the model to an upward-leaping animation, simulating a jump.
func jump() -> void:
	_state_machine.travel("Jump")


## Sets the model to a downward animation, imitating a fall.
func fall() -> void:
	_state_machine.travel("Fall")


## Sets the model to an edge-grabbing animation.
func edge_grab() -> void:
	_state_machine.travel("EdgeGrab")


## Sets the model to a wall-sliding animation.
func wall_slide() -> void:
	_state_machine.travel("WallSlide")


## Plays a one-shot front-flip animation.
## This animation does not play in parallel with other states.
func flip() -> void:
	_animation_tree.set(_flip_shot_path, AnimationNodeOneShot.ONE_SHOT_REQUEST_FIRE)


## Makes a victory sign.
func victory_sign() -> void:
	_state_machine.travel("VictorySign")
