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
            
            _cameraDirection = _cameraTransform.transform.forward;
            _cameraDirection.y = 0;
            _cameraDirection.Normalize();


            _strafeDirection = transform.right;
            _strafeDirection.y = 0;
            _strafeDirection.Normalize();


            transform.rotation = Quaternion.LookRotation(_cameraDirection);
            float curXSpeed = 0;
            float curYSpeed = 0;
            if (Input.GetAxis("Vertical") < 0 ) curXSpeed = _backwardWalkSpeed * Input.GetAxis("Vertical");
            if (Input.GetAxis("Vertical") > 0 ) curXSpeed = _walkSpeed * Input.GetAxis("Vertical");
            curYSpeed = _strafeSpeed * Input.GetAxis("Horizontal");

            _moveDirection = (_cameraDirection * curXSpeed) + (_strafeDirection * curYSpeed);

            _playerController.SimpleMove(_moveDirection);    


            if (Input.GetKey(KeyCode.LeftShift)) _playerAnimator.SetBool("Crouch",true);
            else _playerAnimator.SetBool("Crouch", false);
            _playerAnimator.SetFloat("WalkSpeed", curXSpeed);
            _playerAnimator.SetFloat("StrafeSpeed", curYSpeed);
            if (Input.GetKeyDown(KeyCode.Tab)) Cursor.lockState =  CursorLockMode.None;
        }


        #endregion

        #region Main methods

        #endregion

        #region Utils

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Vector3.zero, _cameraDirection);
        }

        #endregion

        #region Privates & Protected

        [SerializeField] float _walkSpeed;
        [SerializeField] float _backwardWalkSpeed;
        [SerializeField] float _strafeSpeed;
        [SerializeField] CharacterController _playerController;
        Vector3 _moveDirection;
        Transform _cameraTransform;
        Vector3 _cameraDirection;
        Animator _playerAnimator;
        Vector3 _strafeDirection;

        #endregion
    }

}
