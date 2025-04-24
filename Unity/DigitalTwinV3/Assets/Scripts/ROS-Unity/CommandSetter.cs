using UnityEngine;
using Unity.Robotics.UrdfImporter.Control;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class CommandSetter : MonoBehaviour
{
    [Tooltip("Instruction to control the robot: Up, Down, Left, or Right")]
    [SerializeField]  public string instruction = "u";

    [Tooltip("Reference to the Controller script on the robot")]
    public Controller robotController;

    private void Start()
    {
        Debug.Log("Command: " + instruction);
    }

    void Update()
    {
        if (robotController != null && !string.IsNullOrEmpty(instruction))
        {
            robotController.InputCommand = instruction;
        }
    }



}
