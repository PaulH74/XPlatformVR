using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

namespace XPlatformVR
{
    /// <summary>
    /// This class is responsible for creating a room on Photon Network (if master client: first player to connect) or else joining the room
    /// that the master client has created, allowing players to join a networked session in game.
    /// </summary>
    public class LauncherPUN : MonoBehaviourPunCallbacks
    {
        #region Private and Public Attributes
        // Public Attributes
        public const string ROOM_NAME = "MP_Scene";
        public GameObject loadingUI;
        public GameObject connectingUI;
        public Slider loadingSlider;

        // Private Attributes
        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.  NOTE: max is 20 users")]
        [SerializeField]
        private byte _MaxPlayersPerRoom = 20;       // Basic free plan allows max 20 ccu (concurrent users) by default
        private string _GameVersion = "1";          // Set to 1 by default, unless we need to make breaking changes on a project that is Live.
        private bool _IsConnecting;
        private const float _LOADING_TIME = 12f;    // Default time to allow long connection time
        private float _ElapsedTime;

        #endregion

        #region Unity Methods
        private void Awake()
        {
            // Critical
            // This makes sure we can use PhotonNetwork.LoadLevel() on the master client 
            // and all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;

            ToggleInfoScreen(false);
            _ElapsedTime = 0f;
        }

        private void Start()
        {
            Connect();
        }

        private void Update()
        {
            if (_ElapsedTime < _LOADING_TIME)
            {
                loadingSlider.value = _ElapsedTime / _LOADING_TIME;
                _ElapsedTime += Time.deltaTime;
            }
            else
            {
                _ElapsedTime = 0f;
                ToggleInfoScreen(true);
            }
        }

        #endregion

        #region Connection Methods
        private void ToggleInfoScreen(bool toggle)
        {
            loadingUI.SetActive(!toggle);
            connectingUI.SetActive(toggle);
        }

        /// <summary>
        /// This method is called via the Launcher UI when the player clicks the 'Play' button. 
        /// (See the On Click () section associated with the UI in the Inspector window).
        /// </summary>
        public void Connect()
        {
            // Set connecting flag - we are wanting to connect
            _IsConnecting = true;

            // Change UI Screen for user feedback
            //StartCoroutine(TriggerUIChange());

            // Check if we are connected to Photon Network (server) and join a random room
            if (PhotonNetwork.IsConnected)
            {
                // Critical
                // First attempt to join a random room
                // If this fails, we'll get notified in OnJoinRandomFailed() callback and we'll create a room.
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                // Critical
                // Connect to the Photon Network (server) 
                PhotonNetwork.GameVersion = _GameVersion;
                PhotonNetwork.ConnectUsingSettings();                           // Set on PhotonServerSettings in unity editor
            }
        }
        #endregion

        #region Photon Callbacks
        public override void OnConnectedToMaster()
        {
            Debug.Log("OnConnectedToMaster() was called by PUN");

            // Check if we are wanting to connect (prevent looping when we disconnect from a room)
            if (_IsConnecting)
            {
                // Critical
                // Attempt to join a potential existing room.
                // If unsuccessful, we'll be called back with OnJoinRandomFailed()
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("OnDisconnected() was called by PUN with reason {0}", cause.ToString());
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("OnJoinRandomFailed() was called by PUN. No random room available, so we'll create one.\n calling PhotonNetwork.CreateRoom()");

            // Critical
            // We failed to join a random room (room may not exist or room may be already full)
            // So, we create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = _MaxPlayersPerRoom });
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom() was called by PUN. Now this client is in a room.");
            Debug.Log(PhotonNetwork.ServerAddress);

            // Critical
            // We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` 
            // to sync our instance scene.
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("Loading VR Room");

                // Critical
                // Load the Room Level.
                PhotonNetwork.LoadLevel(ROOM_NAME);
            }
        }
        #endregion

        #region Co-Routine Method
        //IEnumerator TriggerUIChange()
        //{
        //    yield return new WaitForSeconds(_LOADING_TIME);

        //    ToggleInfoScreen(true);
        //    _ElapsedTime = 0f;
        //}
        #endregion
    }
}