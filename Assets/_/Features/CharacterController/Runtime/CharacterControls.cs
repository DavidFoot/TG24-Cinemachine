using BruteControllerRuntime;
using System;
using UnityEngine;
using UnityEngine.Playables;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace CharacterControllerRuntime
{
    public enum State
    {
        Free,
        Running,
        GoToWall,
        LeaveWall,
        Strangling,
        WallLocked
    }
    public class CharacterControls : MonoBehaviour
    {
        #region Publics


        #endregion

        #region Unity API

        private void Awake()
        {
            _cameraTransform = Camera.main.transform;
            _playerController = GetComponent<CharacterController>();
            _playerAnimator = GetComponent<Animator>();
            Cursor.lockState = CursorLockMode.Locked;
            _currentState = State.Free;
        }

        private void Update()
        {
            CameraDirection();
            SetCharacterDirection(_isCameraLinkedWithPlayerRotation);

            switch (_currentState)
            {
                case State.Free:
                    _frontWall = CheckWallAvailable(_hipsRayCast, 0.5f);
                    IsInAttackRange();
                    _isCameraLinkedWithPlayerRotation = true;
                    _playerAnimator.applyRootMotion = false;
                    _playerAnimator.SetBool("IsStickToWall", false);
                    if (_frontWall && Input.GetKeyDown(KeyCode.Space))
                    {
                        _playerAnimator.SetBool("WallTransition", true);
                        _currentState = State.GoToWall;
                    }
                    break;
                case State.Strangling:
                    //_bruteController.ApplyRootMotion(true);
                    _bruteController.SetDeath();
                    if (_playableDirector.state == PlayState.Paused)
                    {
                        Debug.Log("Fin de Timeline");
                        _currentState = State.Free;
                        _bruteController = null;
                    }
                    break;
                case State.WallLocked:                   
                    _isCameraLinkedWithPlayerRotation = false;
                    _playerAnimator.SetBool("IsStickToWall", true);
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        _playerAnimator.applyRootMotion = false;
                        _playerAnimator.SetBool("WallTransition", false);
                        _currentState = State.LeaveWall;
                    }
                    break;
                case State.GoToWall:
                    _isCameraLinkedWithPlayerRotation = false;
                    _playerAnimator.applyRootMotion = true;
                    if (IsStickedToWall(0.12f))
                    {
                        SwitchCamera();
                        _currentState = State.WallLocked;
                    }
                    else {
                        transform.Rotate(0, Time.deltaTime*90, 0);                       
                    }
                    break;
                case State.LeaveWall:
                    _currentTimer += Time.deltaTime;
                    _isCameraLinkedWithPlayerRotation = false;
                    _playerAnimator.applyRootMotion = true;
                    if (!IsStickedToWall(0.21f) && _currentTimer >= _wallTransitionTimer)
                    {
                        SwitchCamera();
                        _currentState = State.Free;
                        _currentTimer = 0;
                    }
                    break;

                default:  break;
            }
            if (Input.GetKey(KeyCode.LeftShift)) _playerAnimator.SetBool("Crouch", true);
            else _playerAnimator.SetBool("Crouch", false);
            var speedToApply = _walkSpeed;
            if (_playerAnimator.GetBool("Crouch")) speedToApply /= 2;
            _playerController.SimpleMove(_moveDirection * speedToApply);

            TrucDeTest();               
            
            if (Input.GetKeyDown(KeyCode.Tab)) Cursor.lockState = CursorLockMode.None;          
        }

        private void TrucDeTest()
        {
            if ((Physics.OverlapSphere(_leftHandTransform.position, 0.08f, _wallLayerMask).Length > 0) && (Physics.OverlapSphere(_rightHandTransform.position, 0.08f, _wallLayerMask).Length) > 0)
            {
                Debug.Log("Debug Les mains touchent le mur");

            }
        }

        private bool CheckWallAvailable(Transform origin, float distance)
        {
            RaycastHit hit;
            if (Physics.Raycast(origin.position, origin.forward, out hit,  distance, _wallLayerMask))
            {
                _wallNormal = hit.normal;   
                return true;
            }
            return false;
        }

        private bool IsInAttackRange()
        {
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, 2.8f, _enemiesLayerMask);
            if(colliderArray.Length > 0)
            {
                //Debug.Log("A Port�e");
                if (colliderArray[0].gameObject.GetComponent<BruteController>().CanGetStabbed())
                {
                    if (Vector3.SqrMagnitude(transform.position - colliderArray[0].gameObject.transform.position) < 1) _playableDirector.initialTime = 5.5f;

                    _bruteController = colliderArray[0].gameObject.GetComponent<BruteController>();
                    //Debug.Log("CanBeBackstab");
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        //Debug.Log("Play Timeline");
                        _playableDirector.Play();
                        _currentState = State.Strangling;
                    }
                }
                return true;
            }
            return false;
        }

        private bool IsStickedToWall(float distance)
        {
            RaycastHit hit;
            if (Physics.OverlapSphere(_leftHandTransform.position, distance, _wallLayerMask).Length > 0 && Physics.OverlapSphere(_rightHandTransform.position, distance, _wallLayerMask).Length > 0)
            {
                return true;
            }
            return false; 
        }

        private void SwitchCamera()
        {
            _cameraFree.SetActive(!_cameraFree.activeSelf);
            _cameraLock.SetActive(!_cameraLock.activeSelf);
        }

        private void SetCharacterDirection(bool isCameraLinkedWithPlayerRotation = true)
        {
            if (isCameraLinkedWithPlayerRotation)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_cameraForwardOnXZ);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 250f * Time.deltaTime);
            }
            _moveDirection = (_cameraForwardOnXZ * Input.GetAxis("Vertical")) + (_cameraRightOnXZ * Input.GetAxis("Horizontal"));
            _playerAnimator.SetFloat("WalkSpeed", Input.GetAxis("Vertical"));
            _playerAnimator.SetFloat("StrafeSpeed", Input.GetAxis("Horizontal"));
             
            if (_moveDirection.sqrMagnitude > 1 ) _moveDirection = _moveDirection.normalized;   
        }

        private void CameraDirection(bool inversed = false)
        {
            _cameraForwardOnXZ = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized;
            _cameraRightOnXZ = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized;
            if (inversed) _cameraForwardOnXZ *= -1;
        }


        #endregion

        #region Main methods

        #endregion

        #region Utils

        void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(_hipsRayCast.position, _hipsRayCast.position + _hipsRayCast.forward*0.5f);
            Gizmos.DrawLine(_leftHandTransform.position, _leftHandTransform.position + _leftHandTransform.forward*0.1f);
            Gizmos.DrawLine(_rightHandTransform.position, _rightHandTransform.position + _rightHandTransform.forward * 0.1f);
            Gizmos.DrawSphere(_leftHandTransform.position,0.1f);
            Gizmos.DrawSphere(_rightHandTransform.position, 0.1f);
        }

        #endregion

        #region Privates & Protected

        [SerializeField] float _walkSpeed;
        [SerializeField] float _backwardWalkSpeed;
        [SerializeField] float _strafeSpeed;
        [SerializeField] Transform _leftHandTransform;
        [SerializeField] Transform _rightHandTransform;
        [SerializeField] Transform _hipsRayCast;
        [SerializeField] LayerMask _wallLayerMask;
        [SerializeField] LayerMask _enemiesLayerMask;
        [SerializeField] float  _wallTransitionTimer;
        [SerializeField] GameObject _cameraFree;
        [SerializeField] GameObject _cameraLock;
        [SerializeField] PlayableDirector _playableDirector;
        CharacterController _playerController;
        BruteController _bruteController;

        Transform _cameraTransform;
        Vector3 _moveDirection;
        Vector3 _cameraForwardOnXZ;
        Vector3 _cameraRightOnXZ;
        Vector3 _wallNormal;
        Animator _playerAnimator;
        bool _frontWall;
        bool _isCameraLinkedWithPlayerRotation = true;
        float _currentTimer;
        State _currentState;

        #endregion
    }

}
