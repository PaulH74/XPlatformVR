using UnityEngine;
using Photon.Pun;

namespace XPlatformVR
{
    /// <summary>
    /// This class managers the local player's instance over the PUN network and local player's inputs, sending the Transform data of the local player's VR hardware to other
    /// networked players and receiving their data in turn to animate their VR Avatar on the local player's instance
    /// </summary>
    public class PlayerPCMgrPUN : MonoBehaviourPun, IPunObservable
    {
        #region Public and Private Attributes
        [Tooltip("The local player instance. Use this to know if local player is represented in the scene")]
        public static GameObject localPlayerInstance;

        // PC Player Avatar Elements
        [Header("Player Avatar (Displayed to other networked players):")]
        public GameObject headAvatar;

        // Smoothing Variables For Remote Player's Motion
        [Header("Player Avatar Motion Smoothing:")]
        [Tooltip("0: no smoothing, > 0: increased smoothing \n(note: smoothing reduces positional accuracy and increases latency)")]
        [Range(0, 3)]
        public int smoothingFactor;     // Set to 2 as default (based on CUBE use-case tests)
        [Tooltip("Maximum distance (metres) for which to apply smoothing")]
        [Range(0, 3)]
        public float appliedDistance;   // Set to 1 as default (based on CUBE use-case tests)
        private Vector3 correctPlayerHeadPosition = Vector3.zero;
        private Quaternion correctPlayerHeadRotation = Quaternion.identity;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            // Important:
            // used in RoomManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronised
            if (photonView.IsMine)
            {
                localPlayerInstance = gameObject;

                // Don't display our own "player" avatar to ourselves (except for map icon)
                headAvatar.SetActive(false);
            }

            // Critical
            // Don't Destroy on load to prevent player from being destroyed when another player joins / leaves the room
            DontDestroyOnLoad(gameObject);
        }

        // Update each frame
        private void Update()
        {
            if (photonView.IsMine)
            {
                // AUDIO GROUPS: 
                // Allow user to set local group
                // Sets next available group.
                // Remote group players add that group to their listen list.
            }
            else
            {
                // Smooth Remote player's motion on local machine
                SmoothPlayerMotion(ref headAvatar, ref correctPlayerHeadPosition, ref correctPlayerHeadRotation);
            }
        }
        #endregion

        #region Avatar Related Methods
        /// <summary>
        /// Updates player's left hand avatar pose according to boolean inputs.
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="thumbsUp"></param>
        /// <param name="fingerPoint"></param>
        //private void SetLeftHandPose(bool normal, bool thumbsUp, bool fingerPoint)
        //{
        //    _ShowNormalHandPose_LH = normal;
        //    _ShowThumbUpHandPose_LH = thumbsUp;
        //    _ShowFingerPointHandPose_LH = fingerPoint;
        //}

        /// <summary>
        /// Updates player's right hand avatar pose according to boolean inputs.
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="thumbsUp"></param>
        /// <param name="fingerPoint"></param>
        //private void SetRightHandPose(bool normal, bool thumbsUp, bool fingerPoint)
        //{
        //    _ShowNormalHandPose_RH = normal;
        //    _ShowThumbUpHandPose_RH = thumbsUp;
        //    _ShowFingerPointHandPose_RH = fingerPoint;
        //}

        /// <summary>
        /// Applies LERP interpolation to smooth the remote player's game object motion over the network. 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="gameObjectCorrectTransformPosition"></param>
        /// <param name="gameObjectCorrectTransformRotation"></param>
        private void SmoothPlayerMotion(ref GameObject gameObject, ref Vector3 gameObjectCorrectTransformPosition, ref Quaternion gameObjectCorrectTransformRotation)
        {
            // Smoothing variables
            float distance = Vector3.Distance(gameObject.transform.position, gameObjectCorrectTransformPosition);

            if (distance < appliedDistance)
            {
                gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, gameObjectCorrectTransformPosition, Time.deltaTime * smoothingFactor);
                gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, gameObjectCorrectTransformRotation, Time.deltaTime * smoothingFactor);
            }
            else
            {
                gameObject.transform.position = gameObjectCorrectTransformPosition;
                gameObject.transform.rotation = gameObjectCorrectTransformRotation;
            }
        }
        #endregion

        #region PUN RPCs and Serialize View Method
        /// <summary>
        /// Controls the exchange of data between local and remote player's VR data
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsReading)
            {
                // Receive networked player's VR Headset position and rotation data
                correctPlayerHeadPosition = (Vector3)stream.ReceiveNext();
                correctPlayerHeadRotation = (Quaternion)stream.ReceiveNext();
            }
        }
        #endregion
    }
}