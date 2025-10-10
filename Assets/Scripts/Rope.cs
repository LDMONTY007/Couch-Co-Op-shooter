using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


public class Rope : MonoBehaviour
{
    public Transform startTransform;
    public Transform endTransform;

    LineRenderer lineRenderer;

    public int segments = 5;
    public float ropeLength = 10f;

    List<Joint> joints = new List<Joint>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        SetupJoints();
    }



    // Update is called once per frame
    void Update()
    {
        if (lineRenderer != null && startTransform != null && endTransform != null)
        {
            //set the line renderer position count.
            lineRenderer.positionCount = segments;

            //always update the start and end joints to match
            //where their targets are. 
            joints[0].transform.position = startTransform.position;
            joints[segments - 1].transform.position = endTransform.position;

            //make lineRenderer match the actual joint positions.
            for (int i = 0; i < joints.Count; i++)
            {
                lineRenderer.SetPosition(i, joints[i].transform.position);
            }
            
           
        }
    }

    public void SetupJoints()
    {
        //Destroy all joints.
        if (joints.Count > 0)
        {
            for (int i = 0; i < joints.Count; i++)
            {
                Destroy(joints[i].gameObject);
            }
            joints.Clear();
        }

        for (int i = 0; i < segments; i++)
        {
            GameObject temp = new GameObject();
            Rigidbody rb = temp.AddComponent<Rigidbody>();
            //Disable collisions
            rb.detectCollisions = false;

            //add some damping so the 
            //rigidbodies don't move too fast.
            rb.linearDamping = 5f;
            rb.angularDamping = 5f;

            //create the joint.
            Joint joint = temp.AddComponent<ConfigurableJoint>();

            SoftJointLimit limits = new SoftJointLimit();



            //limit the distance the joint is allowed to have.
            limits.limit = ropeLength / segments;
            (joint as ConfigurableJoint).xMotion = ConfigurableJointMotion.Limited;
            (joint as ConfigurableJoint).yMotion = ConfigurableJointMotion.Limited;
            (joint as ConfigurableJoint).zMotion = ConfigurableJointMotion.Limited;
            (joint as ConfigurableJoint).linearLimit = limits;

            //disable autoconfigure anchor as we only want to use the connected body.
            joint.autoConfigureConnectedAnchor = false;

            //if this is the beginning connect it to the
            //start point
            if (i == 0)
            {
                //joint.connectedAnchor = startTransform.position;
                joint.transform.position = startTransform.position;
                rb.isKinematic = true;
            }
            //if this is the end of the rope connect it to the end position
            //and it's previous segment
            else if (i == segments - 1)
            {
                //set connected anchor position
                //joint.connectedAnchor = endTransform.position;
                joint.transform.position = endTransform.position;
                //Connect to previous joint.
                joint.connectedBody = joints[i - 1].GetComponent<Rigidbody>();
                rb.isKinematic = true;
            }
            //for the inner segments just setup the rigidbody
            //and set their initial position.
            else
            {
                joint.connectedBody = joints[i - 1].GetComponent<Rigidbody>();
                //For now just set it to be the start
                //as it'll correct itself on it's own so no need to calculate the
                //correct spawn position.
                joint.transform.position = startTransform.position;
            }
            
            //add the joint.
            joints.Add(joint);
        }
    }
}
