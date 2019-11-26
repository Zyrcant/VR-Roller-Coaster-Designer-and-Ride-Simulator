/*
Copyright ©2017. The University of Texas at Dallas. All Rights Reserved. 

Permission to use, copy, modify, and distribute this software and its documentation for 
educational, research, and not-for-profit purposes, without fee and without a signed 
licensing agreement, is hereby granted, provided that the above copyright notice, this 
paragraph and the following two paragraphs appear in all copies, modifications, and 
distributions. 

Contact The Office of Technology Commercialization, The University of Texas at Dallas, 
800 W. Campbell Road (AD15), Richardson, Texas 75080-3021, (972) 883-4558, 
otc@utdallas.edu, https://research.utdallas.edu/otc for commercial licensing opportunities.

IN NO EVENT SHALL THE UNIVERSITY OF TEXAS AT DALLAS BE LIABLE TO ANY PARTY FOR DIRECT, 
INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING 
OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF THE UNIVERSITY OF TEXAS AT 
DALLAS HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

THE UNIVERSITY OF TEXAS AT DALLAS SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT 
NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
PURPOSE. THE SOFTWARE AND ACCOMPANYING DOCUMENTATION, IF ANY, PROVIDED HEREUNDER IS 
PROVIDED "AS IS". THE UNIVERSITY OF TEXAS AT DALLAS HAS NO OBLIGATION TO PROVIDE 
MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
*/

using UnityEngine;
using System.Collections;

public class VirtualHand_Left : MonoBehaviour
{

    // Enumerate states of virtual hand interactions
    public enum VirtualHandState
    {
        Open,
        Touching,
        Holding
    };

    // Inspector parameters
    [Tooltip("The tracking device used for tracking the real hand.")]
    public CommonTracker tracker;

    [Tooltip("The interactive used to represent the virtual hand.")]
    public Affect hand;

    [Tooltip("The button required to be pressed to grab objects.")]
    public CommonButton button;

    public CommonButton toggleButton;
    
    [Tooltip("The object to spawn when clicking in an empty area.")]
    public GameObject spawnPrefab;

    [Tooltip("The speed amplifier for thrown objects. One unit is physically realistic.")]
    public float speed = 1.0f;

    // Private interaction variables
    VirtualHandState state;
    FixedJoint grasp;
    private GameObject spawnedObject;

    // Modify Space
    private CommonSpace space;

    public bool riding, pressing;

    // Called at the end of the program initialization
    void Start()
    {

        // Set initial state to open
        state = VirtualHandState.Open;

        // Ensure hand interactive is properly configured
        hand.type = AffectType.Virtual;

        space = GameObject.Find("Vive").transform.Find("Vive Input").GetComponent<CommonSpace>();
    }

    void ride() {
                    GameObject rider = GameObject.Find("Rider");
                    GameObject spline = GameObject.Find("Spline");
                    // Transform hmd = space.transform.Find("Vive HMD");
                    Transform eye = space.transform.Find("Vive Camera (eye)");
                    spline.GetComponent<Spline>().CalculateDistance();
                    rider.GetComponent<Rider>().distance = 0;
                    rider.GetComponent<Rider>().velocity = 0.02;
                    rider.GetComponent<Rider>().last_point_index = 0;
                    rider.GetComponent<Rider>().offset = eye.rotation;
                    rider.GetComponent<Rider>().positionOffset = eye.localPosition;
                    
                    // //ROTATION
                    // // Get current head heading in scene (y-only, to avoid tilting the floor)
                    // float offsetXAngle = eye.rotation.eulerAngles.x;
                    // float offsetAngle = eye.rotation.eulerAngles.y;
                    // float offsetZAngle = eye.rotation.eulerAngles.z;
                    // // Now rotate CameraRig in opposite direction to compensate
                    // space.transform.rotation = spline.transform.rotation; 
                    // space.transform.Rotate(-offsetXAngle, -offsetAngle, -offsetZAngle);

                    rider.GetComponent<Transform>().position = spline.transform.position;
                    rider.GetComponent<Transform>().rotation = spline.transform.rotation;
                    rider.GetComponent<Rider>().ridingMode = riding;
    }
    // FixedUpdate is not called every graphical frame but rather every physics frame
    void FixedUpdate()
    {
        Debug.Log(state);

        // If state is open
        if (state == VirtualHandState.Open)
        {
            // If the hand is touching something
            if (hand.triggerOngoing)
            {
                // If object is destroyed, ignore it
                if(!hand.ongoingTriggers[0]) {
                    if (toggleButton.GetPressDown() && !riding && !pressing) {
                        pressing = true;
                        riding = true;
                        ride();
                    } else if(toggleButton.GetPressDown() && riding && !pressing) {
                        pressing = true;
                        riding = false;
                        GameObject.Find("Rider").GetComponent<Rider>().ridingMode = riding;
                    } else if(!toggleButton.GetPressDown()) {
                        pressing = false;
                    }
                    if (toggleButton.GetPressDown()) {
                        Debug.Log("Pressing button");
                    }
                } else {

                }
                // Change state to touching     
                state = VirtualHandState.Touching;
            }

            // Process current open state
            else
            {
                // If left hand button pressed while not touching a cube, start Riding Mode
                if (toggleButton.GetPressDown() && !riding && !pressing)
                {
                    pressing = true;
                    riding = true;
                    ride();
                } else if(toggleButton.GetPressDown() && riding && !pressing) {
                    pressing = true;
                    riding = false;
                    GameObject.Find("Rider").GetComponent<Rider>().ridingMode = riding;
                } else if(!toggleButton.GetPressDown()) {
                    pressing = false;
                }
                if (toggleButton.GetPressDown()) {
                    Debug.Log("Pressing button");
                }
                // Nothing to do for open
            }
        }

        // If state is touching
        else if (state == VirtualHandState.Touching)
        {

            // If the hand is not touching something
            if (!hand.triggerOngoing)
            {

                // Change state to open
                state = VirtualHandState.Open;
            }

            // If the hand is touching something and the button is pressed
            else if (hand.triggerOngoing)
            {
                // If object is destroyed, ignore it
                if(!hand.ongoingTriggers[0]) {
                    if (toggleButton.GetPressDown() && !riding && !pressing)
                    {
                        
                        pressing = true;
                        riding = true;
                        ride();

                    } else if(toggleButton.GetPressDown() && riding && !pressing) {
                        pressing = true;
                        riding = false;
                        GameObject.Find("Rider").GetComponent<Rider>().ridingMode = riding;
                    } else if(!toggleButton.GetPressDown()) {
                        pressing = false;
                    }
                    if (toggleButton.GetPressDown()) {
                        Debug.Log("Pressing button");
                    }
                    return;
                }

                if(button.GetPress()) {
                    // Fetch touched target
                    Collider target = hand.ongoingTriggers[0];
                    
                    if (target.gameObject.GetComponent<Sphere>().type == "Sphere")
                    {
                        var sphere = target.gameObject;
                        if (sphere.GetComponent<FixedJoint>() != null) {
                            state = VirtualHandState.Open;
                            return;
                        }
                        sphere.GetComponentInParent<Spline>().RedrawSplineForDelete(sphere);
                        Destroy(sphere);
                        state = VirtualHandState.Open;
                        return;
                    }
                }


            }

            // Process current touching state
            else
            {

                // Nothing to do for touching
            }
        }

        // If state is holding
        else if (state == VirtualHandState.Holding)
        {

            // If grasp has been broken
            if (grasp == null)
            {

                // Update state to open
                state = VirtualHandState.Open;
            }

            // If button has been released and grasp still exists
            else if (!button.GetPress() && grasp != null)
            {

                // Get rigidbody of grasped target
                Rigidbody target = grasp.GetComponent<Rigidbody>();
                // Break grasp
                DestroyImmediate(grasp);

                // Apply physics to target in the event of attempting to throw it
                target.velocity = hand.velocity * speed;
                target.angularVelocity = hand.angularVelocity * speed;

                // Update state to open
                state = VirtualHandState.Open;
            }

            // Process current holding state
            else
            {

                // Nothing to do for holding
            }
        }
    }
}