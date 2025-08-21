using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static Enemy;

public class Zombie : MonoBehaviour, IDamageable
{
    public float chaseSpeed = 5f;
    public float walkSpeed = 3f;

    public enum EnemyState
    {
        Chase,
        Patrol,
        Attack
    }

    public EnemyState currentState = EnemyState.Patrol;

    public EnemyType type = EnemyType.Civilian;

    //Distance from player
    //to start fleeing.
    public float attackDistance = 3f;

    public float patrolRange = 50;

    private NavMeshAgent _agent;

    public GameObject playerObj;

    public Animator animator;

    private float timeBetweenFleeChecks = 0.1f;

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

    public bool canAttack = true;

    public float attackCooldownTime = 0.67f;

    // Start is called before the first frame update
    void Start()
    {
        layerMask = ~LayerMask.GetMask("Enemy");

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

        //If chasing and outside 1.5* the attack distance
        //then use the chase speed.
        if (currentState == EnemyState.Chase && distance > attackDistance * 1.5f)
        {
            _agent.speed = chaseSpeed;
        }
        //use walk speed otherwise. 
        else if (currentState == EnemyState.Chase && distance < attackDistance * 1.5f)
        {
            _agent.speed = walkSpeed;
        }
        else if (currentState != EnemyState.Chase)
        {
            _agent.speed = walkSpeed;
        }

        //Flee
        if (distance > attackDistance/* && ((currentState == EnemyState.Chase && curTimeSinceLastFlee >= timeBetweenFleeChecks) || (currentState != EnemyState.Chase))*/)
        {

            //if we aren't fleeing 
            //but we're about to,
            //scream.
            if (currentState != EnemyState.Chase)
            {
                //Remove this random scream for now.
                //PlayRandomScream();
            }

            curTimeSinceLastFlee = 0f;

            //Vector from player to us
            Vector3 dirAwayPlayer = transform.position - playerObj.transform.position;

            Vector3 target = playerObj.transform.position + (dirAwayPlayer.normalized * attackDistance / 2f);


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
            currentState = EnemyState.Chase;
        }

        //if we reached our destination and we are chasing,
        //say we are now attacking.
        if (currentState == EnemyState.Chase && DestinationReached(_agent, transform.position) && distance < attackDistance)
        {
            curTimeSinceLastFlee = 0f;
            //for now just go back to patrolling.
            currentState = EnemyState.Attack;
        }
       /* //start fleeing again
        //if we are still to close to the 
        //player and we're about to stop and switch into patrolling.
        else if (currentState == EnemyState.Chase && distance <= attackDistance && Vector3.Distance(transform.position, _agent.pathEndPosition) <= 2f)
        {
            curTimeSinceLastFlee = 0f;

            //Vector from player to us
            Vector3 dirAwayPlayer = transform.position - playerObj.transform.position;

            Vector3 target = transform.position + dirAwayPlayer;

            _agent.SetDestination(target);

            //set to fleeing so we don't
            //patrol
            currentState = EnemyState.Chase;
        }*/

        if (currentState == EnemyState.Attack)
        {
            Debug.Log("HERE!!");
            Attack();

            //if we are still to close to the 
            //player and we're about to stop and switch into patrolling.

/*            curTimeSinceLastFlee = 0f;

            //Vector from player to us
            Vector3 dirAwayPlayer = transform.position - playerObj.transform.position;

            Vector3 target = transform.position + dirAwayPlayer;

            _agent.SetDestination(target);

            //set to fleeing so we don't
            //patrol
            currentState = EnemyState.Chase;*/
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

    public void TakeDamage(float damage, GameObject other)
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

    float attackDamage = 5f;

    public void Attack()
    {
        //Exit this method if we can't attack.
        if (!canAttack)
        {
            return;
        }

        Debug.Log("HERE!!");
        if (Physics.Raycast(transform.position, transform.forward, out var hitInfo, attackDistance, layerMask))
        {
            Debug.Log("HERE!!");
            IDamageable damageable = hitInfo.transform.gameObject.GetComponent<IDamageable>();

            //if we actually hit a damageable.
            if (damageable != null)
                damageable.TakeDamage(attackDamage, gameObject);
        }

        //Start the cooldown.
        StartCoroutine(CooldownCoroutine());
    }

    private IEnumerator CooldownCoroutine()
    {
        //Don't allow attacks until cooldown is over.
        canAttack = false;
        yield return new WaitForSeconds(attackCooldownTime);
        canAttack = true;
    }
}
