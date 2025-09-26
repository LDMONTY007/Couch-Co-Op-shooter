using UnityEngine;

//I didn't come up with this,
//I learned it from here: https://www.youtube.com/watch?v=tI3USKIbnh0
//Same with the shader it uses.
public class GarlicWobble : MonoBehaviour
{
    public float wobbleSpeed = 1f;
    public float recoveryFactor = 1f;
    public float maxWobble = 0.03f;

    public Transform garlicTransform;

    Vector3 wobble = Vector3.zero;

    float wobbleX = 0f;
    float wobbleZ = 0f;

    float wobbleToAddX = 0f;
    float wobbleToAddZ = 0f;

    Vector3 lastRotation;
    Vector3 lastPos;

    float pulse;
    float time = 0.5f;

    public Renderer liquidRenderer;
    public Renderer garlicRenderer;

    //To get angular velocity I referenced this to
    //make sure I was doing it right:
    //https://discussions.unity.com/t/manually-calculate-angular-velocity-of-gameobject/562533/4

    public Vector3 angularVelocity;
    public Vector3 velocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;




        HandleWobble();

        //Set last rotation.
        lastRotation = transform.rotation.eulerAngles;
        //Set last position
        lastPos = transform.position;



    }

    public void HandleWobble()
    {
        //add angular velocity to wobble.
        //wobble += angularVelocity;

        wobbleToAddX = Mathf.Lerp(wobbleToAddX, 0, Time.deltaTime * recoveryFactor);
        wobbleToAddZ = Mathf.Lerp(wobbleToAddZ, 0, Time.deltaTime * recoveryFactor);

        //make a sine wave of decreasing wobble
        pulse = 2 * Mathf.PI * wobbleSpeed;
        wobble.x = wobbleToAddX * Mathf.Sin(pulse * time);
        wobble.z = wobbleToAddZ * Mathf.Sin(pulse * time);

        //Send to the shader
        liquidRenderer.material.SetFloat("_WobbleX", wobble.x);
        liquidRenderer.material.SetFloat("_WobbleZ", wobble.z);

        garlicRenderer.material.SetFloat("_WobbleX", wobble.x);
        garlicRenderer.material.SetFloat("_WobbleZ", wobble.z);

        //Always have the garlic transform rotated the same way relative
        //to the world so it looks like it's floating.
        //TODO: Use some wobble to affect how quickly it rotates.
        garlicTransform.localRotation = Quaternion.Euler(-wobble.x, wobble.z, 0f);

        //Calculate velocity
        angularVelocity = transform.rotation.eulerAngles - lastRotation;
        velocity = (transform.position - lastPos) / Time.deltaTime;

        //add clamped velocity to wobble.
        wobbleToAddX += Mathf.Clamp((velocity.z + (angularVelocity.x * 0.2f)) * maxWobble, -maxWobble, maxWobble);
        wobbleToAddZ += Mathf.Clamp((velocity.x + (angularVelocity.z * 0.2f)) * maxWobble, -maxWobble, maxWobble);

        /* //Lerp wobble back to 0 so it looks like a liquid settling.
         wobble = Vector3.Lerp(wobble, Vector3.zero, Time.deltaTime * 5);

         //If we're close enough to zero 
         //just drop to zero.
         if (wobble.sqrMagnitude <= 0.1f)
         {
             wobble = Vector3.zero;
         }*/
    }

}
