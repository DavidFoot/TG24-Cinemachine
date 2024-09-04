using PatrolRuntime;
using PlasticPipe.PlasticProtocol.Messages;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using static Codice.Client.Common.WebApi.WebApiEndpoints;


namespace BruteControllerRuntime
{
    public enum BruteState
    {
        Idle,
        PatrolState,
        Chasing,
        Fighting,
        Dead
    }

    public class BruteController : MonoBehaviour
    {
        #region Publics

        #endregion

        #region Unity API

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();  
            _currentState = BruteState.PatrolState;
            if(_patrol) _waypoints = _patrol.GetWaypoints();
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            switch (_currentState)
            {
                case BruteState.Idle:
                    if (IdleForSeconds(_idleTimer)) _currentState = BruteState.PatrolState;
                    _canBeStabbed = CanBeBackStab();
                    break;
                case BruteState.PatrolState:
                    _canBeStabbed = CanBeBackStab(); 
                    PatrolThruWaypoints();
                    break;
                case BruteState.Chasing:
                    _canBeStabbed = false;
                    break;
                case BruteState.Fighting:
                    _canBeStabbed = false;
                    break;
                case BruteState.Dead:
                    _animator.SetBool("IsDead",true);
                    _animator.SetFloat("WalkSpeed", 0);
                    _agent.speed = 0;
                    Destroy(GetComponent<CharacterController>());              
                    break;
                default: break;
            }
            
        }

        #endregion

        #region Main methods

        private bool CanBeBackStab()
        {
            var _playerVector = transform.position - _player.transform.position;
            if (Vector3.SqrMagnitude(_playerVector) < _strangleDistance * _strangleDistance)
            {
                var normalizedDirection = Vector3.Normalize(_playerVector);
                var backward = -transform.forward;
                var playerFront = _player.transform.forward;
                if (Vector3.Dot(backward, normalizedDirection) < _strangleBackAngle && Vector3.Dot(playerFront, normalizedDirection) > _strangleFrontAngle)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetDeath()
        {
            _currentState = BruteState.Dead;
        }

        public bool CanGetStabbed()
        {
            return _canBeStabbed;
        }

        public void ApplyRootMotion(bool value)
        {
            if (_animator)
            {
                _animator.applyRootMotion = value;
            }
        }

        private bool IdleForSeconds(float seconds)
        {
            _timer += Time.deltaTime;
            if(_timer < seconds) return false; 
            _timer = 0;
            return true;
        }

        private void PatrolThruWaypoints() {
            if (_waypoints.Count > 0)
            {
                if (_index == _waypoints.Count) _index = 0;
                _currentWaypoint = _waypoints[_index];
                _agent.speed = _patrolSpeed;
                _agent.SetDestination(_currentWaypoint.position);
                if (Vector3.SqrMagnitude(_currentWaypoint.position - transform.position) <= 1f)
                {
                    _agent.speed = 0;
                    _currentState = BruteState.Idle;
                    _index++;
                }
                _animator.SetFloat("WalkSpeed", _agent.speed);
            }
        }

        #endregion

        #region Utils

        #endregion

        #region Privates & Protected

        [SerializeField] float  _idleTimer = 2.5f;
        [SerializeField] Patrol _patrol ;
        [SerializeField] float  _patrolSpeed ;
        [SerializeField] float  _strangleDistance;
        [SerializeField] GameObject _player;
        [SerializeField] float _strangleBackAngle = -0.65f;
        [SerializeField] float _strangleFrontAngle = 0.85f;
        [SerializeField] bool _canBeStabbed;

        Animator _animator ;
        int _index = 0;
        List<Transform> _waypoints;
        Transform _currentWaypoint;
        NavMeshAgent _agent;
        BruteState _currentState;
        float _timer;

        #endregion
    }

}
