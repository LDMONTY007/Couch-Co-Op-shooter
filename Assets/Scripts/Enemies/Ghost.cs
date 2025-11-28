using UnityEngine;
using static ScoreData;
using UnityEngine.AI;
using System.Collections;
using static UnityEngine.GraphicsBuffer;
using NUnit.Framework.Interfaces;

public class Ghost : MonoBehaviour, IDamageable
{
    public float chaseSpeed = 5f;
    public float walkSpeed = 3f;

    public enum EnemyState
    {
        Idle,
        Stun,
        Chase,
        Patrol,
        Attack
    }

    public EnemyState currentState = EnemyState.Patrol;

    public EnemyType enemyType = EnemyType.Ghost;

    public float baseScore = 50;

    //Distance from player
    //to start fleeing.
    public float attackDistance = 3f;

    public float patrolRange = 50;

    //The radius to become aggro on a player
    //when standing idle.
    public float aggroRadius = 25f;

    public GameObject playerObj;

    public Animator animator;

    public GameObject bloodParticlesPrefab;

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

    Coroutine curStunCoroutine;

    public void Die()
    {
        if (lastAttacker != null)
        {
            //Increment the last attacker's 
            //zombie kill count.
            //lastAttacker.zombieKillCount++;
            //For now we don't do it this way,
            //because we'll add score every time we take damage.
            /*//Tell the player they recieved a score from
            //this object's position so we spawn the score popping up
            //on screen.
            lastAttacker.OnReceiveScore(transform.position, 50);*/
        }

        //TODO: Code dying.
        //Spawn a blood splatter where this enemy was or the corresponding death effect.
        Debug.Log("DEAD");
        Destroy(gameObject);
    }

    private PlayerController lastAttacker = null;

    public bool canAttack = true;

    public float attackCooldownTime = 0.67f;



    public PlayerController FindTargetPlayer()
    {
        PlayerController closestPlayer = PlayerManager.instance.playerList[0];
        float dist = Mathf.Infinity;

        //search for the closest player.
        //TODO: In the future
        //add a sensor to this object
        //that only allows the enemy
        //to check if the player is within their view.
        foreach (PlayerController p in PlayerManager.instance.playerList)
        {
            if (Vector3.Distance(p.transform.position, transform.position) < dist)
            {
                //if the player is dead, ignore them.
                if (p.isDead)
                    continue;
                closestPlayer = p;
                dist = Vector3.Distance(p.transform.position, transform.position);
            }
        }


        //in the future use some code
        //to store how much damage the last attacker
        //has done and decide that when they've done
        //a certain amount of damage we'll switch
        //to targeting them. 
        //otherwise we should target the nearest player
        //to us.
        //We also ignore aggro radius with last attacker code.
        if (lastAttacker != null && lastAttacker == closestPlayer)
        {
            return lastAttacker;
        }

        //Only return a target player
        //if they're within the aggro radius for targeting.
        if (dist < aggroRadius)
        {
            return closestPlayer;
        }
        else
        {
            return null;
        }


    }

    public struct KinematicSteeringOutput
    {
        public Vector3 velocity;
    }

    public float rotationSpeed = 5f;

    //when didArrive = true,
    //we are idle.
    public bool didArrive = true;

    public float currentSpeed = 0f;

    Vector3 targetPos = Vector3.zero;

    KinematicSteeringOutput lastOutput = new KinematicSteeringOutput();

    public KinematicSteeringOutput GetSteering()
    {
        //Create structure for output
        KinematicSteeringOutput steering = new KinematicSteeringOutput();

        //get direction to the target
        steering.velocity = targetPos - transform.position;

        /*//Check if we're within radius of satisfaction
        if (steering.velocity.magnitude < attackDistance)
        {
            //Say we arrived
            didArrive = true;

            //Return empty steering
            return new KinematicSteeringOutput();
        }*/

        //We need to move our target,
        //we want to get there in timeToTarget seconds
        //so divide total velocity required to reach target
        //by the time.
        steering.velocity = steering.velocity.normalized * currentSpeed;

        //Clamp to max speed
        //steering.velocity = Vector3.ClampMagnitude(steering.velocity, maxSpeed);

        //Set y velocity to zero as we don't
        //want to move vertically.
        steering.velocity.y = 0;

        //This isn't the intended use of lerp, 
        //but creates a simple way for us to
        //smoothly rotate while moving
        //without creating a more complex system.
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(steering.velocity), rotationSpeed * Time.deltaTime);

        //output the steering
        return steering;
    }

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
    }

    // Update is called once per frame
    void Update()
    {
        //do ghost bobbing up and down.
        HandleBobbing();

        //Handle the animations.
        HandleAnimations();

        //don't do anything when the player isn't here.
        //try to get any players.
        PlayerController controller = FindTargetPlayer();
        if (controller != null)
            playerObj = controller.gameObject;

        //playerObj will only be null
        //if that player is dead 
        //or if this zombie hasn't 
        //aggroed onto any player yet.
        //otherwise once a player becomes aggroed
        //by a zombie the zombie will pursue them
        //until one of them is dead.
        if (playerObj == null)
        {
            currentState = EnemyState.Idle;
            //Return so we idle if a player was not
            //found in our target radius.
            return;
        }

       
        

        float distance = Vector3.Distance(transform.position, playerObj.transform.position);

        //If chasing and outside 1.5* the attack distance
        //then use the chase speed.
        if (currentState == EnemyState.Chase && distance > attackDistance * 1.5f)
        {
            currentSpeed = chaseSpeed;
        }
        //use walk speed otherwise. 
        else if (currentState == EnemyState.Chase && distance < attackDistance * 1.5f)
        {
            currentSpeed = walkSpeed;
        }
        else if (currentState != EnemyState.Chase)
        {
            currentSpeed = walkSpeed;
        }



        //Chase check
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

            targetPos = playerObj.transform.position + (dirAwayPlayer.normalized * attackDistance / 2f);


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

            

            //set to chase
            currentState = EnemyState.Chase;
            //say we haven't arrived yet
            didArrive = false;
        }

        //if we reached our destination and we are chasing,
        //or we are close enough to the player and we are chasing,
        //say we are now attacking.
        if (currentState == EnemyState.Chase && didArrive || currentState == EnemyState.Chase && distance < attackDistance)
        {
            curTimeSinceLastFlee = 0f;
            //go to attack state
            currentState = EnemyState.Attack;
            //say we arrived
            didArrive = true;
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
        if (currentState == EnemyState.Patrol && didArrive)
        {
            HandlePatrol();
        }


        //if we haven't arrived yet.
        if (!didArrive)
        {
            lastOutput = GetSteering();
            //Add velocity to the player's position * Time.DeltaTime so it scales with framerate
            transform.position += lastOutput.velocity * Time.deltaTime;
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

            targetPos = point;
        }
    }

    public void HandleAnimations()
    {
        if (animator == null)
        {
            return;
        }

        switch (currentState)
        {
            case EnemyState.Chase:
                //if moving play chase animation.
                if (!didArrive)
                {
                    animator.SetBool("Chase", true);
                }
                else
                {
                    animator.SetBool("Chase", false);
                }

                animator.SetBool("Stun", false);
                break;
            case EnemyState.Stun:
                animator.SetBool("Chase", false);
                animator.SetBool("Stun", true);
                break;
            default:
                //Make sure to set chase to false otherwise.
                animator.SetBool("Chase", false);
                animator.SetBool("Stun", false);
                break;
        }

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

    public void TakeDamage(DamageData damageData)
    {
        //if we're invincible, 
        //then exit this method.
        if (invincible)
        {
            return;
        }

        //Say what the last attacker was
        //if it was a player.
        //this helps us identify who gets credit
        //for kills.
        PlayerController p = damageData.other.GetComponent<PlayerController>();
        if (p != null)
            lastAttacker = p;

        // Apply the damage
        curHealth -= damageData.damage;

        if (curStunCoroutine != null)
            StopCoroutine(curStunCoroutine);
        //Stun this zombie using the given weapon's stun time.
        curStunCoroutine = StartCoroutine(StunCoroutine(damageData.stunTime));

        if (damageData.stunTime > 0 && animator != null)
        {
            
            //Make the zombie react to being hit,
            //then say they are stunned.
            animator.SetTrigger("HitReact");
            animator.SetBool("Stun", true);

        }

        Debug.LogWarning("TOOK DAMAGE");

        //Spawn the particle system here with it's orientation
        //matching the damageData hit normal at the damageData hit point.
        Instantiate(bloodParticlesPrefab, damageData.point, Quaternion.LookRotation(damageData.normal));
        //When the particle system ends it will destroy itself.


        //TODO:
        //Add some screen shake

        //Add some knockback to the player from the hit.
    }

    public ScoreData[] TakeDamageScored(DamageData damageData)
    {
        //if we're invincible, 
        //then exit this method.
        if (invincible)
        {
            return null;
        }

        //Say what the last attacker was
        //if it was a player.
        //this helps us identify who gets credit
        //for kills.
        PlayerController p = damageData.other.GetComponent<PlayerController>();
        if (p != null)
            lastAttacker = p;

        // Apply the damage
        curHealth -= damageData.damage;

        if (curStunCoroutine != null)
            StopCoroutine(curStunCoroutine);
        //Stun this zombie using the given weapon's stun time.
        curStunCoroutine = StartCoroutine(StunCoroutine(damageData.stunTime));

        if (damageData.stunTime > 0 && animator != null)
        {
            //Make the zombie react to being hit,
            //then say they are stunned.
            animator.SetTrigger("HitReact");
            animator.SetBool("Stun", true);

        }


        //Spawn the particle system here with it's orientation
        //matching the damageData hit normal at the damageData hit point.
        Instantiate(bloodParticlesPrefab, damageData.point, Quaternion.LookRotation(damageData.normal));
        //When the particle system ends it will destroy itself.


        //TODO:
        //Add some screen shake

        //Add some knockback to the player from the hit.

        //Return the scoreData
        ScoreData sd = new ScoreData(baseScore, damageData.damageType, this.enemyType, transform.position);
        return new ScoreData[] { sd };
    }

    float attackDamage = 5f;

    public void Attack()
    {
        //Exit this method if we can't attack.
        if (!canAttack)
        {
            return;
        }


        if (Physics.Raycast(transform.position, transform.forward, out var hitInfo, attackDistance, layerMask))
        {
            IDamageable damageable = hitInfo.transform.gameObject.GetComponent<IDamageable>();

            //We ignore when an enemy gets a score.
            //if we actually hit a damageable.
            if (damageable != null)
            {
                damageable.TakeDamage(new DamageData() { damage = attackDamage, stunTime = 0.3f, other = gameObject, point = hitInfo.point, normal = hitInfo.normal });

                

                if (animator != null)
                {
                    //Call the code to do the attack animation
                    if (Random.Range(0, 2) == 0)
                    {
                        animator.SetTrigger("Attack0");
                    }
                    else
                    {
                        animator.SetTrigger("Attack1");
                    }
                }
                



            }

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

    private IEnumerator StunCoroutine(float stunTime)
    {
        currentState = EnemyState.Stun;
        stunned = true;

        //make sure the agent is on a nav mesh and enabled.
        if (!didArrive)
        {
            didArrive = true;
        }


        yield return new WaitForSeconds(stunTime);
        stunned = false;
        currentState = EnemyState.Patrol;
        //Say we haven't arrived so we start
        //the movement state machine again.
        didArrive = false;

        //Set current stun coroutine back to null.
        curStunCoroutine = null;

    }

    private void OnDestroy()
    {
        //TODO:
        //Do some death animations.

        //Stop the stun coroutine in case it is still active.
        if (curStunCoroutine != null)
            StopCoroutine(curStunCoroutine);
    }



    float startY = 1f;

    bool reverse = false;

    float curTime = 0f;

    float totalTime = 1f;

    float topOffset = 0.5f;

    float bottomOffset = -0.5f;

    public void HandleBobbing()
    {
        curTime += Time.deltaTime;


        if (curTime < totalTime)
        {
            if (!reverse)
                transform.position = new Vector3(transform.position.x, startY + Mathf.SmoothStep(bottomOffset, topOffset, curTime / totalTime), transform.position.z);
            else
                transform.position = new Vector3(transform.position.x, startY + Mathf.SmoothStep(topOffset, bottomOffset, curTime / totalTime), transform.position.z);
        }
        else
        {
            reverse = !reverse;
            curTime = 0;
        }

    }
}
