using Godot;

public partial class GobotSkin : Node3D
{
    [Export]
    public MeshInstance3D GobotModel { get; set; }
    
    [Signal] 
    public delegate void FootStepEventHandler();

    private AnimationTree _animationTree;
    private AnimationNodeStateMachinePlayback _stateMachine;
    private string _flipShotPath = "parameters/FlipShot/request";

    public override void _Ready()
    {
        _animationTree = GetNode<AnimationTree>("%AnimationTree");
        _stateMachine = (AnimationNodeStateMachinePlayback)
            _animationTree.Get("parameters/StateMachine/playback");
    }

    //use the functions below to change the current animation
    public void Idle()
    {
        _stateMachine.Travel("Idle");
    }

    public void Run()
    {
        _stateMachine.Travel("Run");
    }

    public void Jump()
    {
        _stateMachine.Travel("Jump");
    }

    public void Fall()
    {
        _stateMachine.Travel("Fall");
    }

    public void EdgeGrab()
    {
        _stateMachine.Travel("EdgeGrab");
    }

    public void WallSlide()
    {
        _stateMachine.Travel("WallSlide");
    }

    public void Flip()
    {
        _animationTree.Set(_flipShotPath, (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }

    public void VictorySign()
    {
        _stateMachine.Travel("VictorySign");
    }
}
