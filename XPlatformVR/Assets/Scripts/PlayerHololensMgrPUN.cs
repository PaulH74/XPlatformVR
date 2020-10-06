using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
//using Photon.Voice.Unity;
//using Photon.Voice.PUN;

namespace XPlatformVR
{
    /// <summary>
    /// This class managers the local player's instance over the PUN network and local player's inputs, sending the Transform data of the local player's VR hardware to other
    /// networked players and receiving their data in turn to animate their VR Avatar on the local player's instance
    /// </summary>
    public class PlayerHololensMgrPUN : MonoBehaviourPun, IPunObservable
    {
        #region Public and Private Attributes
        [Tooltip("The local player instance. Use this to know if local player is represented in the scene")]
        public static GameObject localPlayerInstance;

        // VR Avatar Elements
        [Header("Player Avatar (Displayed to other networked players):")]
        public GameObject headAvatar;
        //public GameObject leftHandAvatar;
        //public GameObject rightHandAvatar;
        //public GameObject mapIcon;
        //public GameObject speechOnBubble;
        //public GameObject speechMutedBubble;
        private Transform localVRHeadset;
        //private Transform localVRControllerLeft;
        //private Transform localVRControllerRight;

        // Hand Gestures
        //[Header("Avatar Hand Poses:")]
        //public SkinnedMeshRenderer poseNormalLH;
        //public SkinnedMeshRenderer poseThumbUpLH;
        //public SkinnedMeshRenderer poseFingerPointLH;
        //public SkinnedMeshRenderer poseNormalRH;
        //public SkinnedMeshRenderer poseThumbUpRH;
        //public SkinnedMeshRenderer poseFingerPointRH;
        //private bool _ShowNormalHandPose_LH;
        //private bool _ShowThumbUpHandPose_LH;
        //private bool _ShowFingerPointHandPose_LH;
        //private bool _ShowNormalHandPose_RH;
        //private bool _ShowThumbUpHandPose_RH;
        //private bool _ShowFingerPointHandPose_RH;

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

        //// Oculus Elements
        //[Header("Local Player's Oculus VR (MUST set to INACTIVE in prefab):")]
        //public GameObject CameraRig;

        //// Tool Elements
        //[Header("Player Tools:")]
        //public ToolSpawner toolSpawner;

        // Voice Elements
        //private int _CurrentAvailableLocalGroupNumber;
        //private const byte _REMOTE_GROUP = 1;        // Listens to Remote group and ALL local groups (in list), transmits to Remote group
        //private List<byte> _LocalGrouplist;             // Listens to Remote group, transmits to own local group
        //private Recorder _RecorderPUN;
        ////private Speaker _SpeakerPUN;
        //private bool _IsLocal;                          // False = Remote group member, True = Local group member (physicallly linked)
        //private bool _VoiceOn;
        //public bool VoiceOn
        //{
        //    get { return _VoiceOn; }
        //}
        #endregion

        #region Unity Methods
        private void Awake()
        {
            // Important:
            // used in RoomManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronised
            if (photonView.IsMine)
            {
                localPlayerInstance = gameObject;

                //// Enable Oculus Camera and controllers (for local player only)
                //CameraRig.SetActive(true);

                localVRHeadset = transform;                 // Get transform data from local VR Headset
                //localVRControllerLeft = transform;
                //localVRControllerRight = transform;

                // Don't display our own "player" avatar to ourselves (except for map icon)
                headAvatar.SetActive(false);
                //leftHandAvatar.SetActive(false);
                //rightHandAvatar.SetActive(false);
                //mapIcon.SetActive(true);

                // Voice Transmission (default state is ON, ALL players remote)
                //_CurrentAvailableLocalGroupNumber = 2;          // First available group number after remote group (1)
                //_LocalGrouplist = new List<byte>();             // to contain byte values > 1 per local group (up to 255 limit)
                //_RecorderPUN = GetComponent<Recorder>();

                // Hand Gestures (default state)
                //SetLeftHandPose(true, false, false);
                //SetRightHandPose(true, false, false);
            }

            // Critical
            // Don't Destroy on load to prevent player from being destroyed when another player joins / leaves the room
            DontDestroyOnLoad(gameObject);
        }

        //private void Start()
        //{
        //    if (photonView.IsMine)
        //    {
        //        // Subscribe to REMOTE group by default
        //        ////_RecorderPUN.InterestGroup = _REMOTE_GROUP;                                                  // Transmit
        //        ////PhotonVoiceNetwork.Instance.Client.OpChangeGroups(null, new byte[1] { _REMOTE_GROUP });      // Listen
        //        //PhotonVoiceNetwork.Instance.Client.GlobalInterestGroup = _REMOTE_GROUP;

        //        //ToggleVoice();
        //    }
        //}

        // Update each frame
        private void Update()
        {
            if (photonView.IsMine)
            {
                //mapIcon.transform.position = localVRHeadset.position;
                //mapIcon.transform.eulerAngles = new Vector3(0f, localVRHeadset.eulerAngles.y + 180f, 0f);      // Only show y-axis rotation

                // AUDIO GROUPS: 
                // Allow user to set local group
                // Sets next available group.
                // Remote group players add that group to their listen list.
            }
            else
            {
                // Show networked player's current hand pose
                // Left Hand
                //poseNormalLH.enabled = _ShowNormalHandPose_LH;
                //poseThumbUpLH.enabled = _ShowThumbUpHandPose_LH;
                //poseFingerPointLH.enabled = _ShowFingerPointHandPose_LH;
                // Right Hand
                //poseNormalRH.enabled = _ShowNormalHandPose_RH;
                //poseThumbUpRH.enabled = _ShowThumbUpHandPose_RH;
                //poseFingerPointRH.enabled = _ShowFingerPointHandPose_RH;

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

        //#region Photon Voice Methods
        ///// <summary>
        ///// Toggles a player's Voice Transmission On / Off
        ///// </summary>
        //public void ToggleVoice()
        //{
        //    _VoiceOn = !_VoiceOn;
        //    _RecorderPUN.TransmitEnabled = _VoiceOn;

        //    photonView.RPC("ShowMutedBubble", RpcTarget.Others, !_VoiceOn);
        //}

        //public void SetLocalPlayerGroup()
        //{
        //    // Change remote status to local
        //    _IsLocal = true;

        //    // Assign current available group number as my new local group
        //    byte myLocalGroup = (byte)_CurrentAvailableLocalGroupNumber;

        //    // Sync new group over network
        //    photonView.RPC("AssignNewLocalGroup", RpcTarget.AllBuffered, _CurrentAvailableLocalGroupNumber);

        //    // Re-subscribe to transmit to LOCAL group by default (not remote)
        //    // Note: we are still listening to remote group (and remote group will add our group to their listening groups)
        //    _RecorderPUN.InterestGroup = myLocalGroup;
        //}
        //#endregion

        #region PUN RPCs and Serialize View Method
        //[PunRPC]
        //private void AssignNewLocalGroup(int groupNum)
        //{
        //    // Add assigned group number to list of local groups
        //    _LocalGrouplist.Add((byte)groupNum);

        //    // Change next available group number
        //    _CurrentAvailableLocalGroupNumber = groupNum + 1;

        //    //// Add new local group as a listening group (for ALL REMOTE players only)
        //    if (!_IsLocal)
        //    {
        //        /* The following code may not be needed:
        //         * additional interest groups can simply be added, according to Photon Docs...need to test to be sure
                 
        //         * This code block creates a new array, adding the new local group to the list - may not be required
                    
        //            // Create temporay byte array for group storage
        //            byte[] interestGroups = new byte[_LocalGrouplist.Count + 1];        // +1 to also incorporate remote group

        //            // Add remote group to interest groups array
        //            interestGroups[0] = _RemoteGroup;       

        //            // Add all stored local groups to interest groups array
        //            for (int i = 0; i < _LocalGrouplist.Count; i++)
        //            {
        //                interestGroups[i + 1] = _LocalGrouplist[i];
        //            }

        //            // Update remote player's subscription to all new interest groups
        //            PhotonVoiceNetwork.Instance.Client.OpChangeGroups(null, interestGroups);
        //          */

        //        // Add new local group to interest groups
        //        PhotonVoiceNetwork.Instance.Client.OpChangeGroups(null, new byte[1] { (byte)groupNum });
        //    }
        //}

        //[PunRPC]
        //private void ShowMutedBubble(bool show)
        //{
        //    speechMutedBubble.SetActive(show);
        //}

        /// <summary>
        /// Controls the exchange of data between local and remote player's VR data
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="info"></param>
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //if (stream.IsWriting)
            //{
            //    // Send local VR Headset position and rotation data to networked player
            //    stream.SendNext(localVRHeadset.position);
            //    stream.SendNext(localVRHeadset.rotation);
            //    //stream.SendNext(localVRControllerLeft.position);
            //    //stream.SendNext(localVRControllerLeft.rotation);
            //    //stream.SendNext(localVRControllerRight.position);
            //    //stream.SendNext(localVRControllerRight.rotation);
            //    //stream.SendNext(_ShowNormalHandPose_LH);
            //    //stream.SendNext(_ShowThumbUpHandPose_LH);
            //    //stream.SendNext(_ShowFingerPointHandPose_LH);
            //    //stream.SendNext(_ShowNormalHandPose_RH);
            //    //stream.SendNext(_ShowThumbUpHandPose_RH);
            //    //stream.SendNext(_ShowFingerPointHandPose_RH);
            //    //stream.SendNext(mapIcon.transform.position);
            //    //stream.SendNext(mapIcon.transform.rotation);

            //    //if (!_VoiceOn)
            //    //{
            //    //    stream.SendNext(_VoiceOn);                                  // Do not show "Speaker Bubble" icon when "Muted"
            //    //}
            //    //else
            //    //{
            //    //    stream.SendNext(_RecorderPUN.VoiceDetector.Detected);      // Toggle "Speaker Bubble" on / off when speaking / quiet
            //    //}
            //}
            if (stream.IsReading)
            {
                // Receive networked player's VR Headset position and rotation data
                correctPlayerHeadPosition = (Vector3)stream.ReceiveNext();
                correctPlayerHeadRotation = (Quaternion)stream.ReceiveNext();
                //leftHandAvatar.transform.position = (Vector3)stream.ReceiveNext();
                //leftHandAvatar.transform.rotation = (Quaternion)stream.ReceiveNext();
                //rightHandAvatar.transform.position = (Vector3)stream.ReceiveNext();
                //rightHandAvatar.transform.rotation = (Quaternion)stream.ReceiveNext();
                //_ShowNormalHandPose_LH = (bool)stream.ReceiveNext();
                //_ShowThumbUpHandPose_LH = (bool)stream.ReceiveNext();
                //_ShowFingerPointHandPose_LH = (bool)stream.ReceiveNext();
                //_ShowNormalHandPose_RH = (bool)stream.ReceiveNext();
                //_ShowThumbUpHandPose_RH = (bool)stream.ReceiveNext();
                //_ShowFingerPointHandPose_RH = (bool)stream.ReceiveNext();
                //mapIcon.transform.position = (Vector3)stream.ReceiveNext();
                //mapIcon.transform.rotation = (Quaternion)stream.ReceiveNext();
                //speechOnBubble.SetActive((bool)stream.ReceiveNext());         // Show network players' "Speech Bubble" when they are talking
            }
        }
        #endregion
    }
}