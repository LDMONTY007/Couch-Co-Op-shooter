using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

//This is what we should use for decision making in our system:
//https://www.reddit.com/r/godot/comments/xgrk0g/goap_goaloriented_action_planning_is_absolutely/
//Goal-Oriented Action Planning (GOAP) is a great way to make an extendable system for AI it seems.
//It'll exhibit complex behaviors and be able to have extensions made to it quite easily.
//I want to do this because our enemies will have very specific conditions and these should help 
//us get those. 
//Like Ghosts need to slowly approach towards the player which is a simple behavior.
//But if a player is using a light based weapon we should have the ghost get nervous and decide
//how beneficial it would be to approach a player and when to back away from a player.
//Stuff like that.
//https://web.archive.org/web/20230912145018/https://alumni.media.mit.edu/~jorkin/goap.html
//But Remember LD, let's not overcomplicate this.
//You could easily make a specific class for each different enemy in the game
//and instead later make more complex behavior.
//This is the way to go, make the different classes for each different AI
//and code them the way you need, then when you have working stuff make something more complex.
//But for now, seriously, stick with more basic stuff.

public class Enemy : MonoBehaviour, IDamageable
{
    //https://www.youtube.com/watch?v=Zjlg9F3FRJs

    //used to check if we are playing twice in a row.
    public static AudioClip lastCivilianClipPlayed = null;

    public static AudioClip lastBirdClipPlayed = null;

    public enum EnemyType
    {
        Civilian,
        Bird
    }

    public enum EnemyState
    {
        Flee,
        Patrol
    }

    public EnemyState currentState = EnemyState.Patrol;

    public EnemyType type = EnemyType.Civilian;

    //Distance from player
    //to start fleeing.
    public float fleeDistance;

    public float patrolRange = 50;

    private NavMeshAgent _agent;

    public GameObject playerObj;

    public Animator animator;

    private float timeBetweenFleeChecks = 0.5f;

    private float curTimeSinceLastFlee = 0f;

    //public static AudioClipHolder civilianClipHolder;

    //public static AudioClipHolder birdClipHolder;

    public static AudioClip[] birdScreamingAudioClips;

    public static AudioClip[] civilianScreamingAudioClips;

    int lastScreamingAudioClipIndex;

    //Where we scream from.
    public AudioSource audioSource;

    int layerMask;

    #region health vars
    [Header("Health Variables")]

    //This has to be a const so we can init the field with it properly.
    public const float maxHealth = 100;

    private float _curHealth = maxHealth;

    public float curHealth
    {
        get
        {
            return _curHealth;
        }

        set
        {
            _curHealth = Mathf.Max(value, 0);
            if (curHealth <= 0)
            {
                Die();
            }
        }
    }

    #endregion

    public bool invincible = false;

    private bool _stunned = false;

    //used after being hit to prevent the player from attacking immediately.
    private bool stunned
    {
        get
        {
            return _stunned;

        }
        set
        {
            //Update the UI for the stunned popup.
            //UIManager.Instance.playerUIManager.UpdateStunnedPopup(value);
            _stunned = value;
        }
    }

    public void Die()
    {
        //TODO: Code dying.
        //Spawn a blood splatter where this enemy was or the corresponding death effect.
        Debug.Log("DEAD");
        Destroy(gameObject);
    }


    // Start is called before the first frame update
    void Start()
    {
        layerMask = ~LayerMask.GetMask("Enemy", "Ground");

        /*//Get the audio clip holders.
        AudioClipHolder[] audioClipHolders = FindObjectsOfType(typeof(AudioClipHolder)) as AudioClipHolder[];

        foreach (AudioClipHolder holder in audioClipHolders)
        {
            if (holder.enemyType == EnemyType.Civilian)
            {
                civilianClipHolder = holder;
            }
            else if (holder.enemyType == EnemyType.Bird)
            {
                birdClipHolder = holder;
            }

        }

        civilianScreamingAudioClips = civilianClipHolder.clips;
        birdScreamingAudioClips = birdClipHolder.clips;*/

        _agent = GetComponent<NavMeshAgent>();
        PlayerController controller = FindFirstObjectByType<PlayerController>();
        if (controller != null)
        playerObj = controller.gameObject;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //don't do anything when the player isn't here.
        if (playerObj == null)
        {
            //try to get any players.
            PlayerController controller = FindFirstObjectByType<PlayerController>();
            if (controller != null)
                playerObj = controller.gameObject;
            return;
        }

        float distance = Vector3.Distance(transform.position, playerObj.transform.position);

        //Flee
        if (distance < fleeDistance && ((currentState == EnemyState.Flee && curTimeSinceLastFlee >= timeBetweenFleeChecks) || (currentState != EnemyState.Flee)))
        {

            //if we aren't fleeing 
            //but we're about to,
            //scream.
            if (currentState != EnemyState.Flee)
            {
                //Remove this random scream for now.
                //PlayRandomScream();
            }

            curTimeSinceLastFlee = 0f;

            //Vector from player to us
            Vector3 dirAwayPlayer = transform.position - playerObj.transform.position;

            Vector3 target = transform.position + dirAwayPlayer;


            /*            float radius = 1f;

                        Vector3 randOffset = Random.insideUnitSphere.normalized * radius;
                        randOffset.y = 0;

                        Vector3 newTarget = transform.position + randOffset;

                        Collider[] colliders = Physics.OverlapSphere(newTarget*//*target + transform.transform.up * radius / 2f*//*, radius, layerMask);

                        //find a place that isn't colliding with anything and go there.
                        while (colliders.Length > 0)
                        {
                            randOffset = Random.insideUnitSphere.normalized * radius;
                            randOffset.y = 0;

                            newTarget = transform.position + randOffset;
                            newTarget.y = 0;
                        }*/

            _agent.SetDestination(target);

            //set to fleeing so we don't
            //patrol
            currentState = EnemyState.Flee;
        }

        //if we reached our destination and we are fleeing,
        //say we are no longer fleeing.
        if (currentState == EnemyState.Flee && DestinationReached(_agent, transform.position) && distance > fleeDistance)
        {
            curTimeSinceLastFlee = 0f;
            //for now just go back to patrolling.
            currentState = EnemyState.Patrol;
        }
        //start fleeing again
        //if we are still to close to the 
        //player and we're about to stop and switch into patrolling.
        else if (currentState == EnemyState.Flee && distance <= fleeDistance && Vector3.Distance(transform.position, _agent.pathEndPosition) <= 2f)
        {
            curTimeSinceLastFlee = 0f;

            //Vector from player to us
            Vector3 dirAwayPlayer = transform.position - playerObj.transform.position;

            Vector3 target = transform.position + dirAwayPlayer;

            _agent.SetDestination(target);

            //set to fleeing so we don't
            //patrol
            currentState = EnemyState.Flee;
        }

        //only patrol
        //if our state is patrolling
        //and we've already reached our destination.
        if (currentState == EnemyState.Patrol && DestinationReached(_agent, transform.position))
        {
            HandlePatrol();
        }

        if (animator != null)
        {
            HandleAnimation();
        }
    }

    public void HandlePatrol()
    {
        float distance = Vector3.Distance(transform.position, playerObj.transform.position);

        //Vector from player to us
        Vector3 dirAwayPlayer = transform.position - playerObj.transform.position;

        Vector3 offset = Vector3.zero;

        if (distance <= patrolRange)
        {
            offset = dirAwayPlayer.normalized * (patrolRange - distance);
        }

        //if offset is not zero
        //patrol centerpoint will be opposite direction of
        //the player. 
        //otherwise it will be
        //the current position
        Vector3 centerPoint = transform.position + offset;

        Vector3 point;
        if (RandomPoint(centerPoint, patrolRange, out point)) //pass in our centre point and radius of area
        {
            Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f); //so you can see with gizmos

            _agent.SetDestination(point);
        }
    }

    public void HandleAnimation()
    {
        //set the speed for the animator visuals
        animator.SetFloat("Speed", _agent.velocity.magnitude);
        animator.SetFloat("MotionSpeed", 1f);
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {

        Vector3 randomPoint = center + Random.insideUnitSphere * range; //random point in a sphere 
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas)) //documentation: https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html
        {
            //the 1.0f is the max distance from the random point to a point on the navmesh, might want to increase if range is big
            //or add a for loop like in the documentation
            result = hit.position;
            return true;
        }



        result = Vector3.zero;
        return false;
    }


    //https://discussions.unity.com/t/ai-navmesh-agent-path-status-completed-issues/902035/3
    //  Check if the model is moving on the NavMesh
    public static bool DestinationReached(NavMeshAgent agent, Vector3 actualPosition)
    {
        //  because takes some time to update the remainingDistance and will return a wrong value
        if (agent.pathPending)
        {
            return Vector3.Distance(actualPosition, agent.pathEndPosition) <= agent.stoppingDistance;
        }
        else
        {
            return (agent.remainingDistance <= agent.stoppingDistance);
        }
    }

    /*public void PlayRandomScream()
    {
        if (type == EnemyType.Civilian)
        {
            //Play eating audio
            if (civilianScreamingAudioClips.Length > 0)
            {
                var index = Random.Range(0, civilianScreamingAudioClips.Length);

                //make sure we don't play the same clip twice in a row.
                while (index == lastScreamingAudioClipIndex || lastCivilianClipPlayed == civilianScreamingAudioClips[index] || (Player.instance.memeScream != null && civilianScreamingAudioClips[index] == Player.instance.memeScream))
                {
                    index = Random.Range(0, civilianScreamingAudioClips.Length);
                }

                //just used so we don't play memeScream more than once
                if (civilianScreamingAudioClips[index].name == "MemeScream")
                {
                    Debug.LogWarning("MEME SCREAM USED ONCE");
                    Player.instance.memeScream = civilianScreamingAudioClips[index];
                }

                audioSource.PlayOneShot(civilianScreamingAudioClips[index]);
                lastScreamingAudioClipIndex = index;
                lastCivilianClipPlayed = civilianScreamingAudioClips[index];
                //AudioSource.PlayClipAtPoint(screamingAudioClips[index], transform.TransformPoint(_rb.centerOfMass), FootstepAudioVolume);
            }
        }
        else if (type == EnemyType.Bird)
        {
            //Play eating audio
            if (birdScreamingAudioClips.Length > 0)
            {
                var index = Random.Range(0, birdScreamingAudioClips.Length);

                //make sure we don't play the same clip twice in a row.
                while (index == lastScreamingAudioClipIndex || lastBirdClipPlayed == birdScreamingAudioClips[index])
                {
                    index = Random.Range(0, birdScreamingAudioClips.Length);
                }
                audioSource.PlayOneShot(birdScreamingAudioClips[index]);
                lastScreamingAudioClipIndex = index;
                lastBirdClipPlayed = birdScreamingAudioClips[index];
                //AudioSource.PlayClipAtPoint(screamingAudioClips[index], transform.TransformPoint(_rb.centerOfMass), FootstepAudioVolume);
            }
        }


    }*/

    public void TakeDamage(float damage, float stunTime, GameObject other)
    {
        //if we're invincible, 
        //then exit this method.
        if (invincible)
        {
            return;
        }

        // Apply the damage
        curHealth -= damage;

        //TODO:
        //Add some screen shake

        //Add some knockback to the player from the hit.
    }
}
