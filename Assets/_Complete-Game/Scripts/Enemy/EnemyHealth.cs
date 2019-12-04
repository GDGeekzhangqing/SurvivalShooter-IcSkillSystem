﻿using CabinIcarus.IcSkillSystem.Expansion.Builtin.Component;
using CabinIcarus.IcSkillSystem.Expansion.Runtime.Buffs.Components;
using CabinIcarus.IcSkillSystem.Runtime.Buffs.Entitys;
using CabinIcarus.IcSkillSystem.Runtime.Buffs.Systems.Interfaces;
using UnityEngine;

namespace CompleteProject
{
    public class EnemyHealth : MonoBehaviour,IBuffCreateSystem<IcSkSEntity>
    {
        public int startingHealth = 100;            // The amount of health the enemy starts the game with.

        public float MoveSpeed = 3;
        public int _cuu;
        [field:SerializeField]
        public int currentHealth
        {
            get => (int) GameManager.Manager.BuffManager.GetBuffData<Mechanics>(Entity,HealthBuffIndex).Value;
            set =>  GameManager.Manager.BuffManager.SetBuffData(Entity,new Mechanics()
            {
                Value = Mathf.Clamp(value,0,GameManager.Manager.BuffManager.GetBuffData<Mechanics>(Entity,MaxHealthBuffIndex).Value)
            }, HealthBuffIndex);
        }
        // The current health the enemy has.
        public float sinkSpeed = 2.5f;              // The speed at which the enemy sinks through the floor when dead.
        public int scoreValue = 10;                 // The amount added to the player's score when the enemy dies.
        public AudioClip deathClip;                 // The sound to play when the enemy dies.


        Animator anim;                              // Reference to the animator.
        AudioSource enemyAudio;                     // Reference to the audio source.
        ParticleSystem hitParticles;                // Reference to the particle system that plays when the enemy is damaged.
        CapsuleCollider capsuleCollider;            // Reference to the capsule collider.
        bool isSinking;                             // Whether the enemy has started sinking through the floor.

        #region Buff

        public IcSkSEntity Entity { get; private set; }

        public int HealthBuffIndex { get; } = 0;
        
        public int MaxHealthBuffIndex { get; } = 1;

        public int MoveSpeedBuffIndex { get; } = 2;
        
        #endregion

        private static bool _isAdd;
        void Awake ()
        {
            if (!_isAdd)
            {
                _isAdd = true;
                GameManager.Manager.BuffManager.AddBuffSystem<DeathStruct>(this);
            }

            Entity = GameManager.Manager.EntityManager.CreateEntityAndBind(gameObject, gameObject.GetInstanceID());
            
            GameManager.Manager.BuffManager.AddBuff<Mechanics>(Entity,new Mechanics()
            {
                Value = startingHealth,
                MechanicsType = MechanicsType.Health
            });
            
            GameManager.Manager.BuffManager.AddBuff<Mechanics>(Entity,new Mechanics()
            {
                Value = startingHealth,
                MechanicsType = MechanicsType.MoveSpeed
            });

            GameManager.Manager.BuffManager.AddBuff<Mechanics>(Entity,new Mechanics()
            {
                Value = MoveSpeed,
                MechanicsType = MechanicsType.Health
            });
            
            // Setting up the references.
            anim = GetComponent <Animator> ();
            enemyAudio = GetComponent <AudioSource> ();
            hitParticles = GetComponentInChildren <ParticleSystem> ();
            capsuleCollider = GetComponent <CapsuleCollider> ();
//            // Setting the current health when the enemy first spawns.
//            currentHealth = startingHealth;
            
#if UNITY_EDITOR
            var link = gameObject.AddComponent <BuffEntityLinkComponent>();
            
            link.Init(GameManager.Manager.EntityManager,Entity);
#endif
        }

        void Update ()
        {
            _cuu = currentHealth;
            // If the enemy should be sinking...
            if(isSinking)
            {
                // ... move the enemy down by the sinkSpeed per second.
                transform.Translate (-Vector3.up * sinkSpeed * Time.deltaTime);
            }
        }


        public void TakeDamage(Damage damage, Vector3 hitPoint)
        {
            // If the enemy is dead...
            if (GameManager.Manager.BuffManager.HasBuff(Entity,new DeathStruct()))
            {
                // ... no need to take damage so exit the function.
                return;
            }

            // Play the hurt sound effect.
            enemyAudio.Play ();
            GameManager.Manager.BuffManager.AddBuff(Entity,damage);

//            // Reduce the current health by the amount of damage sustained.
//            currentHealth -= amount;
            
//             Set the position of the particle system to where the hit was sustained.
            hitParticles.transform.position = hitPoint;

            // And play the particles.
            hitParticles.Play();
        }


        void Death ()
        {
            // Turn the collider into a trigger so shots can pass through it.
            capsuleCollider.isTrigger = true;

            // Tell the animator that the enemy is dead.
            anim.SetTrigger ("Dead");

            // Change the audio clip of the audio source to the death clip and play it (this will stop the hurt clip playing).
            enemyAudio.clip = deathClip;
            enemyAudio.Play ();
        }


        public void StartSinking ()
        {
            // Find and disable the Nav Mesh Agent.
            GetComponent <UnityEngine.AI.NavMeshAgent> ().enabled = false;

            // Find the rigidbody component and make it kinematic (since we use Translate to sink the enemy).
            GetComponent <Rigidbody> ().isKinematic = true;

            // The enemy should no sink.
            isSinking = true;

            // Increase the score by the enemy's score value.
            ScoreManager.score += scoreValue;

            // After 2 seconds destory the enemy.
            Destroy (gameObject, 2f);
        }

        public void Create(IcSkSEntity entity, int index)
        {
            var go = GameManager.Manager.EntityManager.FindBindGo(entity);

            if (go)
            {
                var health = go.GetComponent<EnemyHealth>();

                if (health != null)
                {
                    health.Death();
                }
            }
        }
    }
}