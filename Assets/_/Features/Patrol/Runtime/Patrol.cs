using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PatrolRuntime
{
    public class Patrol : MonoBehaviour
    {
        #region Publics

        #endregion

        #region Unity API
        private void Awake()
        {
            ConstrucPatrolNodeList();
        }

        #endregion

        #region Main methods

        public List<Transform> GetWaypoints()
        {
            return _patrolNode;
        }
        private void ConstrucPatrolNodeList()
        {
            _patrolNode = new();
            foreach (Transform _node in transform)
            {
                _patrolNode.Add(_node);
            }
        }

        #endregion

        #region Utils

        void OnDrawGizmos()
        {
            ConstrucPatrolNodeList();
            Gizmos.color = Color.red;
            for (int i = 0; i <= _patrolNode.Count - 1; i++)
            {
                Gizmos.DrawSphere(_patrolNode[i].position, 0.5f);
                if (i == _patrolNode.Count - 1)
                {
                    Gizmos.DrawLine(_patrolNode[i].position, _patrolNode[0].position); continue;
                }
                Gizmos.DrawLine(_patrolNode[i].position, _patrolNode[i + 1].position);
            }
        }

        #endregion

        #region Privates & Protected

        List<Transform> _patrolNode;

        #endregion
    }

}
