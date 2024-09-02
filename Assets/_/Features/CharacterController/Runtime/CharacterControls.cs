using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace CharacterControllerRuntime
{
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
        }

        private void Update()
        {
            GetMoveDirections();
            float curXSpeed, curYSpeed;
            MoveCharacter(out curXSpeed, out curYSpeed);
            _frontWall = CheckWallAvailable(_hipsRayCast, 1f);
            Debug.Log($"_frontWall: {_frontWall}");


            if (_backWall && Input.GetKeyDown(KeyCode.Space))
            {
                _playerAnimator.SetBool("goToWall", false);
                _backWall = false;
            }
            if (_frontWall && Input.GetKeyDown(KeyCode.Space)){ 
                _playerAnimator.SetBool("goToWall", true);
                _backWall = true;
            }

            if (Input.GetKey(KeyCode.LeftShift)) _playerAnimator.SetBool("Crouch", true);
            else _playerAnimator.SetBool("Crouch", false);
            _playerAnimator.SetFloat("WalkSpeed", curXSpeed);
            _playerAnimator.SetFloat("StrafeSpeed", curYSpeed);
            if (Input.GetKeyDown(KeyCode.Tab)) Cursor.lockState = CursorLockMode.None;
        }

        private bool CheckWallAvailable(Transform origin, float distance)
        {
            RaycastHit hit;
            return Physics.Raycast(origin.position, origin.forward, distance, _wallLayerMask);
        }

        private void MoveCharacter(out float curXSpeed, out float curYSpeed)
        {
            transform.rotation = Quaternion.LookRotation(_cameraDirection);
            curXSpeed = 0;
            curYSpeed = 0;
            if (Input.GetAxis("Vertical") < 0) curXSpeed = _backwardWalkSpeed * Input.GetAxis("Vertical");
            if (Input.GetAxis("Vertical") > 0) curXSpeed = _walkSpeed * Input.GetAxis("Vertical");
            curYSpeed = _strafeSpeed * Input.GetAxis("Horizontal");

            _moveDirection = (_cameraDirection * curXSpeed) + (_strafeDirection * curYSpeed);

            _playerController.SimpleMove(_moveDirection);
        }

        private void GetMoveDirections()
        {
            _cameraDirection = _cameraTransform.transform.forward;
            _cameraDirection.y = 0;
            _cameraDirection.Normalize();
            _strafeDirection = transform.right;
            _strafeDirection.y = 0;
            _strafeDirection.Normalize();
        }


        #endregion

        #region Main methods

        #endregion

        #region Utils

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(_hipsRayCast.position, _hipsRayCast.position + _hipsRayCast.forward);
        }

        #endregion

        #region Privates & Protected

        [SerializeField] float _walkSpeed;
        [SerializeField] float _backwardWalkSpeed;
        [SerializeField] float _strafeSpeed;
        [SerializeField] CharacterController _playerController;
        [SerializeField] Transform _headRayCast;
        [SerializeField] Transform _hipsRayCast;
        [SerializeField] LayerMask _wallLayerMask;
        
        Vector3 _moveDirection;
        Transform _cameraTransform;
        Vector3 _cameraDirection;
        Animator _playerAnimator;
        Vector3 _strafeDirection;
        bool _frontWall;
        bool _backWall;


        #endregion
    }

}
