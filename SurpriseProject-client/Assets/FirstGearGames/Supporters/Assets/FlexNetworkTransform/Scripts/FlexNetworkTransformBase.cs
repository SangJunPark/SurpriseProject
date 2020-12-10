﻿using FirstGearGames.Utilities.Editors;
using FirstGearGames.Utilities.Maths;
using FirstGearGames.Utilities.Objects;
using Mirror;
using System;
using UnityEngine;

namespace FirstGearGames.Mirrors.Assets.FlexNetworkTransforms
{


    public abstract class FlexNetworkTransformBase : NetworkBehaviour
    {
        #region Types.
        /// <summary>
        /// Space types to use when getting or setting platform data.
        /// </summary>
        private enum PlatformSpaces
        {
            Disabled = 0,
            Local = 1,
            World = 2
        }
        public class PlatformData
        {
            /// <summary>
            /// Platform.
            /// </summary>
            public NetworkIdentity Identity;
            /// <summary>
            /// For spectators this is the transform to move towards. For owner this is where their transform is in localspace to the platform.
            /// </summary>
            public Transform Target = null;
        }
        /// <summary>
        /// Extrapolation for the most recent received data.
        /// </summary>
        protected class ExtrapolationData
        {
            public float Remaining;
            public Vector3 Position;
        }
        /// <summary>
        /// Move rates for the most recent received data.
        /// </summary>
        protected struct MoveRateData
        {
            public float Position;
            public float Rotation;
            public float Scale;
        }
        /// <summary>
        /// Data used to manage moving towards a target.
        /// </summary>
        protected class TargetSyncData
        {
            public TargetSyncData(TransformSyncData goalData, MoveRateData moveRates, ExtrapolationData extrapolationData)
            {
                GoalData = goalData;
                MoveRates = moveRates;
                Extrapolation = extrapolationData;
            }

            /// <summary>
            /// Transform goal data for this update.
            /// </summary>
            public readonly TransformSyncData GoalData;
            /// <summary>
            /// How quickly to move towards each transform property.
            /// </summary>
            public MoveRateData MoveRates;
            /// <summary>
            /// How much extrapolation time remains.
            /// </summary>
            public readonly ExtrapolationData Extrapolation;
        }
        /// <summary>
        /// Ways to synchronize datas.
        /// </summary>
        [System.Serializable]
        private enum SynchronizeTypes : int
        {
            Normal = 0,
            NoSynchronization = 1
        }
        /// <summary>
        /// Interval types to determine when to synchronize data.
        /// </summary>
        [System.Serializable]
        private enum IntervalTypes : int
        {
            Timed = 0,
            FixedUpdate = 1
        }
        #endregion

        #region Public.
        /// <summary>
        /// Dispatched when server receives data from a client while using client authoritative.
        /// </summary>
        public event Action<ReceivedClientData> OnClientDataReceived;
        /// <summary>
        /// Transform to monitor and modify.
        /// </summary>
        public abstract Transform TargetTransform { get; }
        /// <summary>
        /// PlatformData for the platform this object resides on.
        /// </summary>
        public PlatformData Platform { get; private set; } = new PlatformData();
        /// <summary>
        /// Sets which platform this transform is on.
        /// </summary>
        /// <param name="platform"></param>
        protected void SetPlatformInternal(NetworkIdentity platform)
        {
            uint netId = (platform == null) ? 0 : platform.netId;
            UpdatePlatform(netId);
        }

        #endregion

        #region Serialized.
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("True to synchronize using localSpace rather than worldSpace. If you are to child this object throughout it's lifespan using worldspace is recommended. However, when using worldspace synchronization may not behave properly on VR. LocalSpace is the default.")]
        [SerializeField]
        private bool _useLocalSpace = true;
        /// <summary>
        /// True to synchronize using localSpace rather than worldSpace. If you are to child this object throughout it's lifespan using worldspace is recommended. However, when using worldspace synchronization may not behave properly on VR. LocalSpace is the default.
        /// </summary>
        protected bool UseLocalSpace { get { return _useLocalSpace; } }
        /// <summary>
        /// How to operate synchronization timings. Timed will synchronized every specified interval while FixedUpdate will synchronize every FixedUpdate.
        /// </summary>
        [Tooltip("How to operate synchronization timings. Timed will synchronized every specified interval while FixedUpdate will synchronize every FixedUpdate.")]
        [SerializeField]
        private IntervalTypes _intervalType = IntervalTypes.Timed;
        /// <summary>
        /// True to favor performance over update times. This is ideal when transform updates can be delayed very slightly.
        /// </summary>
        [Tooltip("True to favor performance over update frequency. This is ideal when transform updates can be delayed very slightly.")]
        [SerializeField]
        private bool _favorPerformance = true;
        /// <summary>
        /// How often to synchronize this transform.
        /// </summary>
        [Tooltip("How often to synchronize this transform.")]
        [Range(0.01f, 0.5f)]
        [SerializeField]
        private float _synchronizeInterval = 0.1f;
        /// <summary>
        /// True to synchronize using the reliable channel. False to synchronize using the unreliable channel. Your project must use 0 as reliable, and 1 as unreliable for this to function properly. This feature is not supported on TCP transports.
        /// </summary>
        [Tooltip("True to synchronize using the reliable channel. False to synchronize using the unreliable channel. Your project must use 0 as reliable, and 1 as unreliable for this to function properly.")]
        [SerializeField]
        private bool _reliable = true;
        /// <summary>
        /// True to resend transform data with every unreliable packet. At the cost of bandwidth this will ensure smoother movement on very unstable connections but generally is not needed.
        /// </summary>
        [Tooltip("True to resend transform data with every unreliable packet. At the cost of bandwidth this will ensure smoother movement on very unstable connections but generally is not needed.")]
        [SerializeField]
        private bool _resendUnreliable = false;
        /// <summary>
        /// True to synchronize data anytime it has changed. False to allow greater differences before synchronizing.
        /// </summary>
        [Tooltip("True to synchronize data anytime it has changed. False to allow greater differences before synchronizing.")]
        [SerializeField]
        private bool _preciseSynchronization = false;
        /// <summary>
        /// How far in the past objects should be for interpolation. Higher values will result in smoother movement with network fluctuations but lower values will result in objects being closer to their actual position. Lower values can generally be used for longer synchronization intervalls.
        /// </summary>
        [Tooltip("How far in the past objects should be for interpolation. Higher values will result in smoother movement with network fluctuations but lower values will result in objects being closer to their actual position. Lower values can generally be used for longer synchronization intervals.")]
        [Range(0.00f, 0.5f)]
        [SerializeField]
        private float _interpolationFallbehind = 0.06f;
        /// <summary>
        /// How long to extrapolate when data is expected but does not arrive. Smaller values are best for fast synchronization intervals. For precision or fast reaction games you may want to use no extrapolation or only one or two synchronization intervals worth. Extrapolation is client-side only.
        /// </summary>
        [Tooltip("How long to extrapolate when data is expected but does not arrive. Smaller values are best for fast synchronization intervals. For precision or fast reaction games you may want to use no extrapolation or only one or two synchronization intervals worth. Extrapolation is client-side only.")]
        [Range(0f, 5f)]
        [SerializeField]
        private float _extrapolationSpan = 0f;
        /// <summary>
        /// Teleport the transform if the distance between received data exceeds this value. Use 0f to disable.
        /// </summary>
        [Tooltip("Teleport the transform if the distance between received data exceeds this value. Use 0f to disable.")]
        [SerializeField]
        private float _teleportThreshold = 0f;
        /// <summary>
        /// True if using client authoritative movement.
        /// </summary>
        [Tooltip("True if using client authoritative movement.")]
        [SerializeField]
        private bool _clientAuthoritative = true;
        /// <summary>
        /// True to synchronize server results back to owner. Typically used when you are sending inputs to the server and are relying on the server response to move the transform.
        /// </summary>
        [Tooltip("True to synchronize server results back to owner. Typically used when you are sending inputs to the server and are relying on the server response to move the transform.")]
        [SerializeField]
        private bool _synchronizeToOwner = true;
        /// <summary>
        /// Synchronize options for position.
        /// </summary>
        [Tooltip("Synchronize options for position.")]
        [SerializeField]
        private SynchronizeTypes _synchronizePosition = SynchronizeTypes.Normal;
        /// <summary>
        /// Euler axes on the position to snap into place rather than move towards over time.
        /// </summary>
        [Tooltip("Euler axes on the rotation to snap into place rather than move towards over time.")]
        [SerializeField]
        [BitMask(typeof(Axes))]
        private Axes _snapPosition = (Axes)0;
        /// <summary>
        /// Sets SnapPosition value. For internal use only. Must be public for editor script.
        /// </summary>
        /// <param name="value"></param>
        public void SetSnapPosition(Axes value) { _snapPosition = value; }
        /// <summary>
        /// Synchronize states for rotation.
        /// </summary>
        [Tooltip("Synchronize states for position.")]
        [SerializeField]
        private SynchronizeTypes _synchronizeRotation = SynchronizeTypes.Normal;
        /// <summary>
        /// Euler axes on the rotation to snap into place rather than move towards over time.
        /// </summary>
        [Tooltip("Euler axes on the rotation to snap into place rather than move towards over time.")]
        [SerializeField]
        [BitMask(typeof(Axes))]
        private Axes _snapRotation = (Axes)0;
        /// <summary>
        /// Sets SnapRotation value. For internal use only. Must be public for editor script.
        /// </summary>
        /// <param name="value"></param>
        public void SetSnapRotation(Axes value) { _snapRotation = value; }
        /// <summary>
        /// Synchronize states for scale.
        /// </summary>
        [Tooltip("Synchronize states for scale.")]
        [SerializeField]
        private SynchronizeTypes _synchronizeScale = SynchronizeTypes.Normal;
        /// <summary>
        /// Euler axes on the scale to snap into place rather than move towards over time.
        /// </summary>
        [Tooltip("Euler axes on the scale to snap into place rather than move towards over time.")]
        [SerializeField]
        [BitMask(typeof(Axes))]
        private Axes _snapScale = (Axes)0;
        /// <summary>
        /// Sets SnapScale value. For internal use only. Must be public for editor script.
        /// </summary>
        /// <param name="value"></param>
        public void SetSnapScale(Axes value) { _snapScale = value; }
        #endregion

        #region Private.
        /// <summary>
        /// Last SyncData sent by client.
        /// </summary>
        private TransformSyncData _clientSyncData = null;
        /// <summary>
        /// Last SyncData sent by server.
        /// </summary>
        private TransformSyncData _serverSyncData = null;
        /// <summary>
        /// TargetSyncData to move between.
        /// </summary>
        private TargetSyncData _targetData = null;
        /// <summary>
        /// Last SequenceId sent by the client.
        /// </summary>
        private uint _lastClientSentSequenceId = 0;
        /// <summary>
        /// Last SequenceId sent by the server.
        /// </summary>
        private uint _lastServerSentSequenceId = 0;
        /// <summary>
        /// Last SequenceId received from client.
        /// </summary>
        private uint _lastClientReceivedSequenceId = 0;
        /// <summary>
        /// Last SequenceId received from server.
        /// </summary>
        private uint _lastServerReceivedSequenceId = 0;
        /// <summary>
        /// Next time client may send data.
        /// </summary>
        private float _nextClientSendTime = 0f;
        /// <summary>
        /// Next time server may send data.
        /// </summary>
        private float _nextServerSendTime = 0f;
        /// <summary>
        /// When sending data from client, after the transform stops changing and when using unreliable this becomes true while a reliable packet is being sent.
        /// </summary>
        private bool _clientSettleSent = false;
        /// <summary>
        /// When sending data from server, after the transform stops changing and when using unreliable this becomes true while a reliable packet is being sent.
        /// </summary>
        private bool _serverSettleSent = false;
        /// <summary>
        /// Last frame FixedUpdate ran.
        /// </summary>
        private int _lastFixedFrame = -1;
        /// <summary>
        /// TeleportThreshold value squared.
        /// </summary>
        private float _teleportThresholdSquared;
        #endregion

        #region Const.
        /// <summary>
        /// Value to use when comparing positions without precise synchronization.
        /// </summary>
        private const float NEAR_POSITION_TOLERANCE = 0.1f;
        /// <summary>
        /// Value to use when comparing rotations without precise synchronization.
        /// </summary>
        private const float NEAR_ROTATION_TOLERANCE = 1f;
        #endregion

        protected virtual void Awake()
        {
            SetTeleportThresholdSquared();
#if MIRRORNG
            base.NetIdentity.OnStartClient.AddListener(StartClient);
            base.NetIdentity.OnStartAuthority.AddListener(StartAuthority);
            base.NetIdentity.OnStartAuthority.AddListener(StopAuthority);
#endif
        }
        protected virtual void OnDestroy()
        {
            if (Platform.Target != null)
                Destroy(Platform.Target.gameObject);
#if MIRRORNG
            base.NetIdentity.OnStartClient.RemoveListener(StartClient);
            base.NetIdentity.OnStopAuthority.RemoveListener(StartAuthority);
            base.NetIdentity.OnStopAuthority.RemoveListener(StopAuthority);
#endif            
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            bool platformValid = PlatformValid();
            //Write platform id regardless.
            uint platformNetId = (platformValid) ? Platform.Identity.netId : 0;
            writer.WriteUInt32(platformNetId);
            //If valid also write platform target offsets.
            if (platformValid)
            {
                writer.WriteVector3(Platform.Target.localPosition);
                writer.WriteQuaternion(Platform.Target.localRotation);
            }

            return base.OnSerialize(writer, initialState);
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            /* Get current platform. */
            uint platformNetId = reader.ReadUInt32();
            UpdatePlatform(platformNetId);

            /* If a netId was specified regardless of if platform
             * is found or not we must read the pos/rot of platform
             * to maintain serialization order. */
            if (platformNetId != 0)
            {
                Platform.Target.localPosition = reader.ReadVector3();
                Platform.Target.localRotation = reader.ReadQuaternion();

                if (PlatformValid())
                {
                    /* If not authoritative client with client auth, and platform
                     * exist then make a new target data based on transforms offsets
                     * to platform with instant move rates. This is so the transform 
                     * sticks to the platform until new data comes in. */
                    if (!base.hasAuthority)
                    {
                        Vector3 positionOffset = Platform.Target.localPosition;
                        Quaternion rotationOffset = Platform.Target.localRotation;
                        //Generate configured sync properties with platform.
                        SyncProperties sp = ReturnConfiguredSynchronizedProperties();
                        sp |= SyncProperties.Platform;
                        TransformSyncData goalData = new TransformSyncData((byte)sp, 0, positionOffset, rotationOffset, transform.localScale, Platform.Identity.netId);
                        _targetData = new TargetSyncData(goalData, SetInstantMoveRates(), null);
                        /* Force a move towards target. This is because server data can come often come in before
                         * Update runs, resulting in a new target data with improper move rates. */
                        MoveTowardsTargetSyncData();
                    }
                }
            }

            base.OnDeserialize(reader, initialState);
        }

#if MIRROR
        public override void OnStartClient()
        {
            base.OnStartClient();
            StartClient();
        }
#endif
        private void StartClient()
        {
            CheckCreateTransformTargetData();
        }


#if MIRROR
        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            StartAuthority();
        }
#endif
        private void StartAuthority()
        {
            /* If have authority and client authoritative then there is
            * no reason to have a targe data. */
            if (_clientAuthoritative)
                _targetData = null;
        }

#if MIRROR
        public override void OnStopAuthority()
        {
            base.OnStopAuthority();
            StopAuthority();
        }
#endif

        private void StopAuthority()
        {
            CheckCreateTransformTargetData();
        }

        protected virtual void Update()
        {
            CheckSendToServer();
            SnapToPlatform();
            CheckSendToClients();
            MoveTowardsTargetSyncData();
        }

        private void FixedUpdate()
        {
            /* Don't send if the same frame. Since
             * physics aren't actually involved there is
             * no reason to run logic twice on the
             * same frame; that will only hurt performance
             * and the network more. */
            if (Time.frameCount == _lastFixedFrame)
                return;
            _lastFixedFrame = Time.frameCount;

            CheckSendToServer();
            CheckSendToClients();
        }

        /// <summary>
        /// Sets TeleportThresholdSquared value.
        /// </summary>
        private void SetTeleportThresholdSquared()
        {
            if (_teleportThreshold < 0f)
                _teleportThreshold = 0f;

            _teleportThresholdSquared = (_teleportThreshold * _teleportThreshold);
        }

        /// <summary>
        /// Checks if conditions are met to call CreateTransformTargetData.
        /// </summary>
        private void CheckCreateTransformTargetData()
        {
            /* If a client starts without being allowed to move the object a target data
            * must be set using current transform values so that the client may not move
             * the object. */
            /* If target data has not already been received.
             * If not server, since server is boss and shouldn't be blocked.
             * If client does not have authority or 
             * have authority but not client authoritative. */
            if (_targetData == null && !NetworkIsServer() && (!NetworkHasAuthority() || (NetworkHasAuthority() && !_clientAuthoritative)))
                CreateTransformTargetData();
        }

        /// <summary>
        /// Creates target data according to where the transform is currently.
        /// </summary>
        protected void CreateTransformTargetData()
        {
            /* Use a -1 sequence id so that it may be overriden by any
            * new data. */
            TransformSyncData tsd = new TransformSyncData(0, 0,
                TargetTransform.GetPosition(UseLocalSpace), TargetTransform.GetRotation(UseLocalSpace), TargetTransform.GetScale(),
                0
                );

            //Set move rates to move instantly to goal.
            MoveRateData mrd = SetInstantMoveRates();
            //Create new target data without extrpaolation.
            _targetData = new TargetSyncData(tsd, mrd, null);
        }

        /// <summary>
        /// Returns synchronization interval used.
        /// </summary>
        /// <returns></returns>
        private float ReturnSyncInterval()
        {
            return (_intervalType == IntervalTypes.FixedUpdate) ? Time.fixedDeltaTime : _synchronizeInterval;
        }

        /// <summary>
        /// Checks if client needs to send data to server.
        /// </summary>
        private void CheckSendToServer()
        {
            //Timed interval.
            if (_intervalType == IntervalTypes.Timed)
            {
                if (Time.inFixedTimeStep)
                    return;

                if (Time.time < _nextClientSendTime)
                    return;
            }
            //Fixed interval.
            else
            {
                if (!Time.inFixedTimeStep)
                    return;
            }

            //Not using client auth movement.
            if (!_clientAuthoritative)
                return;
            //Only send to server if client.
            if (!NetworkIsClient())
                return;
            //Not authoritative client.
            if (!NetworkHasAuthority())
                return;

            bool platformValid = PlatformValid();
            //Update platform target position to transform.
            if (platformValid)
            {
                Platform.Target.position = transform.position;
                Platform.Target.rotation = transform.rotation;
            }

            SyncProperties sp = ReturnDifferentTransformProperties(_clientSyncData, _preciseSynchronization, platformValid);

            bool useReliable = _reliable;
            if (!CanSendProperties(ref sp, ref _clientSettleSent, ref useReliable))
                return;

            /* This only applies if using interval but
            * add anyway since the math operation is fast. 
            *
            * No reason to favor performance for a single client
            * as the performance differences will only be noticeable
            * from server to client, since it will be potentially
            * updating a lot of clients vs one client updating server. */
            _nextClientSendTime = Time.time + _synchronizeInterval;
            //Add additional sync properties.
            ApplyRequiredSyncProperties(ref sp, false, platformValid);

            PlatformSpaces platformSpace = (platformValid) ? PlatformSpaces.Local : PlatformSpaces.Disabled;
            uint platformNetId = GetPlatformNetworkId(platformValid);
            Vector3 position = GetTransformPosition(platformSpace);
            Quaternion rotation = GetTransformRotation(platformSpace);

            _clientSyncData = new TransformSyncData((byte)sp, _lastClientSentSequenceId,
                position, rotation, TargetTransform.GetScale(),
                platformNetId
                );

            _lastClientSentSequenceId += 1;

            //send to clients.
            if (useReliable)
                CmdSendSyncDataReliable(_clientSyncData);
            else
                CmdSendSyncDataUnreliable(_clientSyncData);
        }

        /// <summary>
        /// Checks if server needs to send data to clients.
        /// </summary>
        private void CheckSendToClients()
        {
            //Timed interval.
            if (_intervalType == IntervalTypes.Timed)
            {
                if (Time.inFixedTimeStep)
                    return;
                if (Time.time < _nextServerSendTime)
                    return;
            }
            //Fixed interval.
            else
            {
                if (!Time.inFixedTimeStep)
                    return;
            }

            //Only send to clients if server.
            if (!NetworkIsServer())
                return;

            /* If favoring performance reset check time regardless if transform moves.
             * Otherwise check time will only reset after transform moves. */
            if (_favorPerformance)
                _nextServerSendTime = Time.time + _synchronizeInterval;

            bool platformValid = PlatformValid();
            /* TargetData is generated when there is a goal to move towards.
             * While null it's safe to assume the transform was snapped or is being controlled
             * as a client host, so data is up to date. So with that in mind,
             * if null we can use current transform values. */
            bool transformAtGoal = (_targetData == null);
            SyncProperties sp;
            /* If at goal then compare transform values
             * against the last sent values, _serverSyncData. */
            if (transformAtGoal)
                sp = ReturnDifferentTransformProperties(_serverSyncData, _preciseSynchronization, platformValid);
            /* If not at goal then compare the last received
             * values, _targetData, against the last sent
             * values, _serverSyncData. */
            else
                sp = ReturnDifferentTargetDataProperties(_serverSyncData, _targetData, _preciseSynchronization);

            bool useReliable = _reliable;
            if (!CanSendProperties(ref sp, ref _serverSettleSent, ref useReliable))
                return;

            /* This only applies if using interval but
            * add anyway since the math operation is fast. */
            if (!_favorPerformance)
                _nextServerSendTime = Time.time + _synchronizeInterval;
            //Add additional sync properties.
            ApplyRequiredSyncProperties(ref sp, false, platformValid);

            /* If not using server sync data then we are using
             * targetdata. This is when running as a client host, and
             * the data is what was received from a spectator. */
            if (!transformAtGoal)
            {
                /* Have to use just calculated sync properties because the sync properties
                 * from server to client can vary from what they were client to server when
                 * using client authority. */
                _serverSyncData = new TransformSyncData((byte)sp, _lastServerSentSequenceId,
                    _targetData.GoalData.Position, _targetData.GoalData.Rotation, _targetData.GoalData.Scale, _targetData.GoalData.PlatformNetId);
            }
            /* If using server data then transform was snapped into position
             * or has authority and is client host.
             * So there is no need to read from targetdata. */
            else
            {
                PlatformSpaces platformSpace = (platformValid) ? PlatformSpaces.Local : PlatformSpaces.Disabled;
                uint platformNetId = GetPlatformNetworkId(platformValid);
                Vector3 position = GetTransformPosition(platformSpace);
                Quaternion rotation = GetTransformRotation(platformSpace);

                _serverSyncData = new TransformSyncData((byte)sp, _lastServerSentSequenceId,
                    position, rotation, TargetTransform.GetScale(),
                    platformNetId
                    );
            }

            _lastServerSentSequenceId += 1;

            //Don't need to send to owner if client authoritative, or server auth and don't sync to owner.
            bool excludeOwner = (_clientAuthoritative || (!_clientAuthoritative && !_synchronizeToOwner));
            //send to clients.
            if (useReliable)
            {
                if (excludeOwner)
                    RpcSendSyncDataReliableExcludeOwner(_serverSyncData);
                else
                    RpcSendSyncDataReliable(_serverSyncData);
            }
            else
            {
                if (excludeOwner)
                    RpcSendSyncDataUnreliableExcludeOwner(_serverSyncData);
                else
                    RpcSendSyncDataUnreliable(_serverSyncData);
            }
        }

        /// <summary>
        /// Updates the platform cache and returns true if platform exist.
        /// </summary>
        private bool UpdatePlatform(uint netId)
        {
            NetworkIdentity currentPlatformIdentity = Platform.Identity;
            //If net id is 0 then platform cannot be looked up.
            if (netId == 0)
            {
                Platform.Identity = null;
                if (Platform.Target != null)
                    Destroy(Platform.Target.gameObject);
            }
            else
            {
                /* True if the update should be blocked. This is to save performance.
                 * 
                 * True if current platform exist, and it's netId matches passed in Id. */
                bool blockUpdate = (Platform.Target != null && currentPlatformIdentity != null && currentPlatformIdentity.netId == netId);
                if (!blockUpdate)
                {
                    //Create Platform target if not present.
                    if (Platform.Target == null)
                    {
                        Platform.Target = new GameObject().transform;
                        Platform.Target.name = (base.hasAuthority) ? "ClientToServer Target" : "ServerToClient Target";
                    }

                    Platform.Identity = NetworkIdentity.spawned[netId];
                    //Child target to new platform.
                    if (PlatformValid())
                    {
                        Platform.Target.SetParent(Platform.Identity.transform);
                        /* Set platform to transform position so that it doesn't interfer
                         * with smoothing between space change. */
                        Platform.Target.position = TargetTransform.GetPosition(false);
                        Platform.Target.rotation = TargetTransform.GetRotation(false);
                    }
                }
            }

            return PlatformValid();
        }
        /// <summary>
        /// Returns if a platform is valid.
        /// </summary>
        /// <returns></returns>
        private bool PlatformValid()
        {
            return (Platform.Identity != null && Platform.Identity.netId != 0);
        }

        /// <summary>
        /// Sets which platform netId to use.
        /// </summary>
        /// <param name="platformNetId"></param>
        private uint GetPlatformNetworkId(bool platformValid)
        {
            //Platform exist.
            if (platformValid)
                return Platform.Identity.netId;
            else
                return 0;
        }

        /// <summary>
        /// Gets the position of the platform if one is used, otherwise uses transform values.
        /// </summary>
        private Vector3 GetTransformPosition(PlatformSpaces platformSpace)
        {
            //Platform exist.
            if (platformSpace != PlatformSpaces.Disabled)
                return (platformSpace == PlatformSpaces.Local) ? Platform.Target.localPosition : Platform.Target.position;
            //No platform.
            else
                return TargetTransform.GetPosition(UseLocalSpace);
        }

        /// <summary>
        /// Gets the rotation of the platform if one is used, otherwise uses transform values.
        /// </summary>
        private Quaternion GetTransformRotation(PlatformSpaces platformSpace)
        {
            //Platform exist.
            if (platformSpace != PlatformSpaces.Disabled)
                return (platformSpace == PlatformSpaces.Local) ? Platform.Target.localRotation : Platform.Target.rotation;
            //No platform.
            else
                return TargetTransform.GetRotation(UseLocalSpace);
        }

        /// <summary>
        /// Applies SyncProperties which are required based on settings.
        /// </summary>
        /// <param name="sp"></param>
        private void ApplyRequiredSyncProperties(ref SyncProperties sp, bool forceAll, bool platformValid)
        {
            /* //FEATURE Always send platform if platform is valid.
             * In the future I'd like to only send if changed. */
            if (forceAll || platformValid)
                sp |= SyncProperties.Platform;

            //If to force all.
            if (forceAll)
            {
                sp |= ReturnConfiguredSynchronizedProperties();
            }
            //If has settled then must include all transform values to ensure a perfect match.
            else if (EnumContains.SyncPropertiesContains(sp, SyncProperties.Settled))
            {
                sp |= ReturnConfiguredSynchronizedProperties();
            }
            //If not reliable must send everything that is generally synchronized.
            else if (!_reliable)
            {
                if (_resendUnreliable)
                    sp |= (ReturnConfiguredSynchronizedProperties() | SyncProperties.Sequenced);
            }

        }

        /// <summary>
        /// Returns properties which are configured to be synchronized. This does not mean all of these properties will send. These only send if using unreliable or if settled to force a synchronization.
        /// </summary>
        /// <returns></returns>
        private SyncProperties ReturnConfiguredSynchronizedProperties()
        {
            SyncProperties sp = SyncProperties.None;

            if (_synchronizePosition == SynchronizeTypes.Normal)
                sp |= SyncProperties.Position;
            if (_synchronizeRotation == SynchronizeTypes.Normal)
                sp |= SyncProperties.Rotation;
            if (_synchronizeScale == SynchronizeTypes.Normal)
                sp |= SyncProperties.Scale;

            return sp;
        }

        /// <summary>
        /// Returns if data updates should send based on SyncProperties, Reliable, and send history.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        private bool CanSendProperties(ref SyncProperties sp, ref bool settleSent, ref bool useReliable)
        {
            //If nothing has changed.
            if (sp == SyncProperties.None)
            {
                /* If reliable is default and there's no extrapolation
                 * then there is no reason to send a settle packet.
                 * This is because extrapolation can overshoot while
                 * waiting for a new packet, but with extrapolation off
                 * the most recent reliable packet is always the latest
                 * data. */
                if (_reliable && _extrapolationSpan == 0f)
                    return false;

                //Settle has already been sent.
                if (settleSent)
                {
                    return false;
                }
                //Settle has not been sent yet.
                else
                {
                    settleSent = true;
                    useReliable = true;
                    sp |= SyncProperties.Settled;
                    return true;
                }
            }
            //Properties need to be synchronized.
            else
            {
                //Unset settled.
                settleSent = false;

                return true;
            }

        }

        /// <summary>
        /// Returns which properties need to be sent to maintain synchronization with the transforms current properties.
        /// </summary>
        /// <returns></returns>
        private SyncProperties ReturnDifferentTransformProperties(TransformSyncData data, bool precise, bool platformValid)
        {
            SyncProperties sp = SyncProperties.None;

            //Data is null, so it's definitely not a match.
            if (data == null)
            {
                ApplyRequiredSyncProperties(ref sp, true, platformValid);
                return sp;
            }

            //Position.
            if (_synchronizePosition == SynchronizeTypes.Normal)
            {
                bool positionMatches = (platformValid) ? PlatformPositionMatches(data, precise) : TransformPositionMatches(data, precise);
                if (!positionMatches)
                    sp |= SyncProperties.Position;
            }
            //Rotation.
            if (_synchronizeRotation == SynchronizeTypes.Normal)
            {
                bool rotationMatches = (platformValid) ? PlatformRotationMatches(data, precise) : TransformRotationMatches(data, precise);
                if (!rotationMatches)
                    sp |= SyncProperties.Rotation;
            }
            //Scale.
            if (_synchronizeScale == SynchronizeTypes.Normal)
            {
                //Platforms don't support scale, so if platform is valid then scale always matches.
                bool scaleMatches = (platformValid) ? true : TransformScaleMatches(data, precise);
                if (!scaleMatches)
                    sp |= SyncProperties.Scale;
            }

            return sp;
        }
        /// <summary>
        /// Returns which properties need to be sent to maintain synchronization with targetData properties.
        /// </summary>
        /// <param name="targetData">When specified data is compared against targetData. Otherwise, data is compared against the transform.</param>
        /// <returns></returns>
        private SyncProperties ReturnDifferentTargetDataProperties(TransformSyncData data, TargetSyncData targetData, bool precise)
        {
            //Cannot compare if either data is null.
            if (data == null || targetData == null)
                return (SyncProperties.Position | SyncProperties.Rotation | SyncProperties.Scale | SyncProperties.Platform);

            SyncProperties sp = SyncProperties.None;
            //Position.
            if (_synchronizePosition == SynchronizeTypes.Normal)
            {
                if (!TargetDataPositionMatches(data, targetData, precise))
                    sp |= SyncProperties.Position;
            }
            //Rotation.
            if (_synchronizeRotation == SynchronizeTypes.Normal)
            {
                if (!TargetDataRotationMatches(data, targetData, precise))
                    sp |= SyncProperties.Rotation;
            }
            //Scale.
            if (_synchronizeScale == SynchronizeTypes.Normal)
            {
                if (!TargetDataScaleMatches(data, targetData, precise))
                    sp |= SyncProperties.Scale;
            }

            return sp;
        }

        /// <summary>
        /// Snaps transform to Platform Target.
        /// </summary>
        private void SnapToPlatform()
        {
            /* Only used to keep transform on the server when
             * as server only. This ensures proper position updates
             * will be sent to clients as needed, and that the object
             * will not slide around on the server. */
            if (!base.isServerOnly)
                return;
            if (!PlatformValid())
                return;

            if (_synchronizePosition != SynchronizeTypes.NoSynchronization)
                TargetTransform.SetPosition(false, Platform.Target.position);
            if (_synchronizeRotation != SynchronizeTypes.NoSynchronization)
                TargetTransform.SetRotation(false, Platform.Target.rotation);
            //Platform support doesn't require scale syncing.
        }


        /// <summary>
        /// Moves towards TargetSyncData.
        /// </summary>
        private void MoveTowardsTargetSyncData()
        {
            //No SyncData to check against.
            if (_targetData == null)
                return;
            /* Client authority but there is no owner.
             * Can happen when client authority is ticked but
            * the server takes away authority. */
            if (NetworkIsServer() && _clientAuthoritative && !NetworkHasOwner() && _targetData != null)
            {
                /* Remove sync data so server no longer tries to sync up to last data received from client.
                 * Object may be moved around on server at this point. */
                _targetData = null;
                return;
            }
            //Client authority, don't need to synchronize with self.
            if (NetworkHasAuthority() && _clientAuthoritative)
                return;
            //Not client authority but also not synchronize to owner.
            if (NetworkHasAuthority() && !_clientAuthoritative && !_synchronizeToOwner)
                return;

            bool platformValid = PlatformValid();
            bool extrapolate = (_targetData.Extrapolation != null && _targetData.Extrapolation.Remaining > 0f);

            /* Move platform target towards goal. */
            if (platformValid)
                TryMovePlatformTarget();

            /* Check if transform is at goal.
             * If Transform is at goal and there
             * is no extrapolation left then exit method. */
            if (TransformAtSyncData(_targetData.GoalData, true, platformValid) && !extrapolate)
                return;
            /* Only move using localspace if configured to local space
             * and if not using a platform. Platform offsets arrive in local space
             * but the transform must move in world space to the platform
             * target. */
            bool useLocalSpace = (UseLocalSpace && !platformValid);

            //Position
            if (_synchronizePosition != SynchronizeTypes.NoSynchronization)
            {
                Vector3 positionGoal = (platformValid) ? Platform.Target.position : _targetData.GoalData.Position;

                /* If platform is valid then use instant move rates. This
                 * is because the platform target is already smooth so
                 * we can snap the transform. */
                float moveRate = (platformValid) ? -1f : _targetData.MoveRates.Position;
                //Instantly.
                if (moveRate == -1f)
                {
                    TargetTransform.SetPosition(useLocalSpace, positionGoal);
                }
                //Over time.
                else
                {
                    //If to extrapolate then overwrite position goal with extrapolation.
                    if (extrapolate)
                        positionGoal = _targetData.Extrapolation.Position;

                    TargetTransform.SetPosition(useLocalSpace,
                        Vector3.MoveTowards(TargetTransform.GetPosition(useLocalSpace), positionGoal, moveRate * Time.deltaTime)
                        );
                }
            }
            //Rotation.
            if (_synchronizeRotation != SynchronizeTypes.NoSynchronization)
            {
                Quaternion rotationGoal = (platformValid) ? Platform.Target.rotation : _targetData.GoalData.Rotation;
                /* If platform is valid then use instant move rates. This
                * is because the platform target is already smooth so
                * we can snap the transform. */
                float moveRate = (platformValid) ? -1f : _targetData.MoveRates.Rotation;
                //Instantly.
                if (moveRate == -1f)
                {
                    TargetTransform.SetRotation(useLocalSpace, rotationGoal);
                }
                //Over time.
                else
                {
                    TargetTransform.SetRotation(UseLocalSpace,
                        Quaternion.RotateTowards(TargetTransform.GetRotation(useLocalSpace), rotationGoal, moveRate * Time.deltaTime)
                        );
                }
            }
            //Scale.
            if (_synchronizeScale != SynchronizeTypes.NoSynchronization)
            {
                Vector3 scaleGoal = _targetData.GoalData.Scale;
                //Instantly.
                if (_targetData.MoveRates.Scale == -1f)
                {
                    TargetTransform.SetScale(scaleGoal);
                }
                //Over time.
                else
                {
                    TargetTransform.SetScale(
                        Vector3.MoveTowards(TargetTransform.GetScale(), scaleGoal, _targetData.MoveRates.Scale * Time.deltaTime)
                        );
                }
            }

            //Remove from remaining extrapolation time.
            if (extrapolate)
                _targetData.Extrapolation.Remaining -= Time.deltaTime;
        }

        /// <summary>
        /// Tries to move the platform target to it's goal position. This method assumes a platform is valid.
        /// </summary>
        private void TryMovePlatformTarget()
        {
            //Always use local space when moving the platform.
            bool useLocalSpace = true;
            //Position
            if (_synchronizePosition != SynchronizeTypes.NoSynchronization)
            {
                //Instant.
                if (_targetData.MoveRates.Position == -1f)
                {
                    Platform.Target.SetPosition(useLocalSpace, _targetData.GoalData.Position);
                }
                //Over time.
                else
                {
                    //Move target to goal.
                    Platform.Target.SetPosition(useLocalSpace,
                        Vector3.MoveTowards(Platform.Target.GetPosition(useLocalSpace), _targetData.GoalData.Position, _targetData.MoveRates.Position * Time.deltaTime)
                        );
                }
            }
            //Rotation.
            if (_synchronizeRotation != SynchronizeTypes.NoSynchronization)
            {
                //Instant.
                if (_targetData.MoveRates.Rotation == -1f)
                {
                    Platform.Target.SetRotation(useLocalSpace, _targetData.GoalData.Rotation);
                }
                //Over time.
                else
                {
                    //Move target to goal.
                    Platform.Target.SetRotation(useLocalSpace,
                        Quaternion.RotateTowards(Platform.Target.GetRotation(useLocalSpace), _targetData.GoalData.Rotation, _targetData.MoveRates.Rotation * Time.deltaTime)
                        );
                }
            }

            //Platform target ignores scale.
        }

        /// <summary>
        /// Returns true if the passed in axes contains all axes.
        /// </summary>
        /// <param name="axes"></param>
        /// <returns></returns>
        private bool SnapAll(Axes axes)
        {
            return (axes == (Axes.X | Axes.Y | Axes.Z));
        }

        /// <summary>
        /// Returns true if the TargetTransform is aligned with data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool TransformAtSyncData(TransformSyncData data, bool precise, bool platformValid)
        {
            if (data == null)
                return false;

            bool positionMatches = (platformValid) ? TransformPositionMatchesPlatform(precise) : TransformPositionMatches(data, precise);
            bool rotationMatches = (platformValid) ? TransformRotationMatchesPlatform(precise) : TransformRotationMatches(data, precise);
            bool scaleMatches = TransformScaleMatches(data, precise);
            return (positionMatches && rotationMatches && scaleMatches);
        }

        #region Position Matches.
        /// <summary>
        /// Returns if the transform position matches platform position.
        /// </summary>
        /// <param name="precise"></param>
        /// <returns></returns>
        private bool TransformPositionMatchesPlatform(bool precise)
        {
            float tolerance = (precise) ? 0f : NEAR_POSITION_TOLERANCE;
            return Vectors.Near(Platform.Target.position, TargetTransform.GetPosition(false), tolerance);
        }
        /// <summary>
        /// Returns if this Platform.Target position matches data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool PlatformPositionMatches(TransformSyncData data, bool precise)
        {
            if (data == null)
                return false;

            float tolerance = (precise) ? 0f : NEAR_POSITION_TOLERANCE;
            return Vectors.Near(Platform.Target.localPosition, data.Position, tolerance);
        }

        /// <summary>
        /// Returns if TargetTransform position matches data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool TransformPositionMatches(TransformSyncData data, bool precise)
        {
            if (data == null)
                return false;

            float tolerance = (precise) ? 0f : NEAR_POSITION_TOLERANCE;
            return Vectors.Near(TargetTransform.GetPosition(UseLocalSpace), data.Position, tolerance);
        }
        /// <summary>
        /// Returns if this TargetData position matches data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool TargetDataPositionMatches(TransformSyncData data, TargetSyncData targetData, bool precise)
        {
            if (data == null || targetData == null)
                return false;

            float tolerance = (precise) ? 0f : NEAR_POSITION_TOLERANCE;
            return Vectors.Near(targetData.GoalData.Position, data.Position, tolerance);
        }
        #endregion

        #region Rotation Matches.
        /// <summary>
        /// Returns if this transform rotation matches platform.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool TransformRotationMatchesPlatform(bool precise)
        {
            float tolerance = (precise) ? 0f : NEAR_ROTATION_TOLERANCE;
            return Quaternions.Near(Platform.Target.rotation, TargetTransform.GetRotation(false), tolerance);
        }
        /// <summary>
        /// Returns if this transform rotation matches data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool PlatformRotationMatches(TransformSyncData data, bool precise)
        {
            if (data == null)
                return false;

            float tolerance = (precise) ? 0f : NEAR_ROTATION_TOLERANCE;
            return Quaternions.Near(Platform.Target.localRotation, data.Rotation, tolerance);
        }
        /// <summary>
        /// Returns if this transform rotation matches data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool TransformRotationMatches(TransformSyncData data, bool precise)
        {
            if (data == null)
                return false;

            float tolerance = (precise) ? 0f : NEAR_ROTATION_TOLERANCE;
            return Quaternions.Near(TargetTransform.GetRotation(UseLocalSpace), data.Rotation, tolerance);
        }
        /// <summary>
        /// Returns if this transform rotation matches data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool TargetDataRotationMatches(TransformSyncData data, TargetSyncData targetData, bool precise)
        {
            if (data == null || targetData == null)
                return false;

            float tolerance = (precise) ? 0f : NEAR_ROTATION_TOLERANCE;
            return Quaternions.Near(targetData.GoalData.Rotation, data.Rotation, tolerance);
        }
        #endregion

        #region Scale Matches.
        /// <summary>
        /// Returns if this transform scale matches data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool TransformScaleMatches(TransformSyncData data, bool precise)
        {
            if (data == null)
                return false;

            float tolerance = (precise) ? 0f : NEAR_POSITION_TOLERANCE;
            return Vectors.Near(TargetTransform.GetScale(), data.Scale, tolerance);
        }
        /// <summary>
        /// Returns if this transform scale matches data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool TargetDataScaleMatches(TransformSyncData data, TargetSyncData targetData, bool precise)
        {
            if (data == null || targetData == null)
                return false;

            float tolerance = (precise) ? 0f : NEAR_POSITION_TOLERANCE;
            return Vectors.Near(targetData.GoalData.Scale, data.Scale, tolerance);
        }
        #endregion

        /// <summary>
        /// Sends SyncData to the server. Only used with client auth.
        /// </summary>
        /// <param name="data"></param>
#if MIRROR
        [Command(channel = 0)]
#elif MIRRORNG
        [ServerRpc(channel = 0)]
#endif
        private void CmdSendSyncDataReliable(TransformSyncData data)
        {
            ClientDataReceived(data);
        }
        /// <summary>
        /// Sends SyncData to the server. Only used with client auth.
        /// </summary>
        /// <param name="data"></param>
#if MIRROR
        [Command(channel = 1)]
#elif MIRRORNG
        [ServerRpc(channel = 1)]
#endif
        private void CmdSendSyncDataUnreliable(TransformSyncData data)
        {
            ClientDataReceived(data);
        }

        /// <summary>
        /// Called on clients when server data is received.
        /// </summary>
        /// <param name="data"></param>
        [Server]
        private void ClientDataReceived(TransformSyncData data)
        {
            //Sent to self.
            if (NetworkHasAuthority())
                return;
            if (OutOfSequence(data, ref _lastClientReceivedSequenceId))
                return;

            //Fill in missing data for properties that werent included in send.
            FillMissingData(data, _targetData);

            /* If platform is valid then set the target transform
            * to values received from the client. */
            bool platformValid = UpdatePlatform(data.PlatformNetId);

            if (OnClientDataReceived != null)
            {
                ReceivedClientData rcd = new ReceivedClientData(ReceivedClientData.DataTypes.Interval, UseLocalSpace, data);
                OnClientDataReceived.Invoke(rcd);

                //If data was nullified then do nothing.
                if (rcd.Data == null || data == null)
                    return;
            }

            /* If server only then snap to target position. 
             * Should I ever add extrapolation on server only
             * then I would need to move smoothly instead and
             * perform extrapolation
             * calculations. */
            if (NetworkIsServerOnly())
            {
                ApplyTransformSnapping(data, true, platformValid);
                //Set instant move rates.
                MoveRateData moveRates = SetInstantMoveRates();
                /* If there is a platform then target data must
                 * be set to keep object on platform. Otherwise
                 * targetdata can be nullified. */
                if (platformValid)
                    _targetData = new TargetSyncData(data, moveRates, null);
                else
                    _targetData = null;
            }
            /* If not server only, so if client host, then set data
             * normally for smoothing. */
            else
            {
                ExtrapolationData extrapolation = null;
                MoveRateData moveRates;
                //If teleporting set move rates to be instantaneous.
                if (ShouldTeleport(data, platformValid))
                {
                    moveRates = SetInstantMoveRates();
                    ApplyTransformSnapping(data, true, platformValid);
                }
                //If not teleporting calculate extrapolation and move rates.
                else
                {
                    extrapolation = SetExtrapolation(data, _targetData, platformValid);
                    moveRates = SetMoveRates(data, platformValid);
                    ApplyTransformSnapping(data, false, platformValid);
                }

                _targetData = new TargetSyncData(data, moveRates, extrapolation);
            }
        }

        /// <summary>
        /// Returns if the transform should teleport.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool ShouldTeleport(TransformSyncData data, bool platformValid)
        {
            if (_teleportThresholdSquared <= 0f)
                return false;

            Vector3 position = (platformValid) ? GetTransformPosition(PlatformSpaces.Local) : GetTransformPosition(PlatformSpaces.Disabled);

            float dist = Vectors.FastSqrMagnitude(position - data.Position);
            return dist >= _teleportThresholdSquared;
        }

        /// <summary>
        /// Returns if the sequence on data is out of order against last sequence and updates last sequence.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="lastSequence"></param>
        /// <returns></returns>
        private bool OutOfSequence(TransformSyncData data, ref uint lastSequence)
        {
            SyncProperties sp = (SyncProperties)data.SyncProperties;
            /* If not reliable then compare sequence id to ensure
             * this did not arrive out of order. */
            if (EnumContains.SyncPropertiesContains(sp, SyncProperties.Sequenced))
            {
                //If data Id is less than last Id then it's old data.
                if (data.SequenceId < lastSequence)
                    return true;

                lastSequence = data.SequenceId;
            }

            //Fall through. If here not out of sequence.
            return false;
        }

        /// <summary>
        /// Sets MoveRates to move instantly.
        /// </summary>
        /// <returns></returns>
        private MoveRateData SetInstantMoveRates()
        {
            return new MoveRateData()
            {
                Position = -1f,
                Rotation = -1f,
                Scale = -1f
            };
        }

        /// <summary>
        /// Sets MoveRates based on data, transform position, and synchronization interval.
        /// </summary>
        /// <param name="data"></param>
        private MoveRateData SetMoveRates(TransformSyncData data, bool platformValid)
        {
            float past = ReturnSyncInterval() + _interpolationFallbehind;
            PlatformSpaces platformSpace = (platformValid) ? PlatformSpaces.Local : PlatformSpaces.Disabled;

            MoveRateData moveRates = new MoveRateData();
            float distance;
            /* Position. */
            Vector3 position = GetTransformPosition(platformSpace);
            distance = Vector3.Distance(position, data.Position);
            moveRates.Position = distance / past;
            /* Rotation. */
            Quaternion rotation = GetTransformRotation(platformSpace);
            distance = Quaternion.Angle(rotation, data.Rotation);
            moveRates.Rotation = distance / past;
            /* Scale. */
            distance = Vector3.Distance(TargetTransform.GetScale(), data.Scale);
            moveRates.Scale = distance / past;

            return moveRates;
        }


        /// <summary>
        /// Sets ExtrapolationExtra using TransformSyncData.
        /// </summary>
        /// <param name="extrapolation"></param>
        /// <param name="data"></param>
        private ExtrapolationData SetExtrapolation(TransformSyncData data, TargetSyncData previousTargetSyncData, bool platformValid)
        {
            ////Feature: can only extrapolate when not on a platform currently.
            //if (!platformValid)
            //    return null;
            /* If platform Id changed. //Feature:
             * Cannot extrapolate when platform Ids change because
             * the space used is changed. */
            if (previousTargetSyncData != null && previousTargetSyncData.GoalData.PlatformNetId != data.PlatformNetId)
                return null;
            //No extrapolation.
            if (_extrapolationSpan == 0f || previousTargetSyncData == null)
                return null;
            //Settled packet.
            if (EnumContains.SyncPropertiesContains((SyncProperties)data.SyncProperties, SyncProperties.Settled))
                return null;

            Vector3 positionDirection = (data.Position - previousTargetSyncData.GoalData.Position);
            //Feature: need to use proper platform spaces if supporting extrapolation on platform.
            Vector3 position = GetTransformPosition(PlatformSpaces.Disabled);
            Vector3 goalDirectionNormalzied = (data.Position - position).normalized;
            /* If direction to goal is different from extrapolation direction
             * then do not extrapolate. This can occur when the extrapolation
             * overshoots. If the extrapolation was to continue like this then
             * it would likely overshoot more and more, becoming extremely
             * offset. */
            if (goalDirectionNormalzied != positionDirection.normalized)
                return null;

            float multiplier = _extrapolationSpan / ReturnSyncInterval();

            return new ExtrapolationData()
            {
                Position = data.Position + (positionDirection * multiplier),
                Remaining = ReturnSyncInterval() + _extrapolationSpan,
            };
        }

        /// <summary>
        /// Snaps transforms to data where snapping is applicable.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="snapAll"></param>
        private void ApplyTransformSnapping(TransformSyncData data, bool snapAll, bool platformValid)
        {
            //Snap platform, then snap transform to platform.
            if (platformValid)
            {
                ApplyTransformSnapping(Platform.Target, data, snapAll, true);
                ApplyTransformSnapping(TargetTransform, (SyncProperties)data.SyncProperties, Platform.Target.position, Platform.Target.rotation, data.Scale, snapAll, false);
            }
            //Just snap transform to data.
            else
            {
                ApplyTransformSnapping(TargetTransform, data, snapAll, platformValid);
            }
        }
        /// <summary>
        /// Snaps the transform to data where snapping is applicable.
        /// </summary>
        /// /// <param name="t"></param>
        /// <param name="data"></param>
        /// <param name="snapAll"></param>
        /// <param name="platformSpace"></param>
        private void ApplyTransformSnapping(Transform t, TransformSyncData data, bool snapAll, bool snappingPlatform)
        {
            ApplyTransformSnapping(t, (SyncProperties)data.SyncProperties, data.Position, data.Rotation, data.Scale, snapAll, snappingPlatform);
        }
        /// <summary>
        /// Snaps the transform to data where snapping is applicable.
        /// </summary>
        /// <param name="targetData">Data to snap from.</param>
        private void ApplyTransformSnapping(Transform t, SyncProperties sp, Vector3 position, Quaternion rotation, Vector3 scale, bool snapAll, bool snappingPlatform)
        {
            if (t == null)
                return;

            //If using local space or platform space is specified.
            bool useLocalSpace = (UseLocalSpace || snappingPlatform);
            /* //TODO allow passing in transform to snap. Allow snapping in world or local space.
             * rework platform code in this method. its broken. */
            if (snapAll || EnumContains.SyncPropertiesContains(sp, SyncProperties.Position))
            {
                //If to snap all.
                if (snapAll || SnapAll(_snapPosition))
                {
                    t.SetPosition(useLocalSpace, position);
                }
                //Snap some or none.
                else
                {
                    //Snap X.
                    if (EnumContains.AxesContains(_snapPosition, Axes.X))
                        t.SetPosition(useLocalSpace, new Vector3(position.x, t.GetPosition(useLocalSpace).y, t.GetPosition(useLocalSpace).z));
                    //Snap Y.
                    if (EnumContains.AxesContains(_snapPosition, Axes.Y))
                        t.SetPosition(useLocalSpace, new Vector3(t.GetPosition(useLocalSpace).x, position.y, t.GetPosition(useLocalSpace).z));
                    //Snap Z.
                    if (EnumContains.AxesContains(_snapPosition, Axes.Z))
                        t.SetPosition(useLocalSpace, new Vector3(t.GetPosition(useLocalSpace).x, t.GetPosition(useLocalSpace).y, position.z));
                }
            }

            /* Rotation. */
            if (snapAll || EnumContains.SyncPropertiesContains(sp, SyncProperties.Rotation))
            {
                //If to snap all.
                if (snapAll || SnapAll(_snapRotation))
                {
                    t.SetRotation(useLocalSpace, rotation);
                }
                //Snap some or none.
                else
                {
                    /* Only perform snap checks if snapping at least one
                     * to avoid extra cost of calculations. */
                    if ((int)_snapRotation != 0)
                    {
                        /* Convert to eulers since that is what is shown
                         * in the inspector. */
                        Vector3 startEuler = t.GetRotation(UseLocalSpace).eulerAngles;
                        Vector3 targetEuler = rotation.eulerAngles;
                        //Snap X.
                        if (EnumContains.AxesContains(_snapRotation, Axes.X))
                            startEuler.x = targetEuler.x;
                        //Snap Y.
                        if (EnumContains.AxesContains(_snapRotation, Axes.Y))
                            startEuler.y = targetEuler.y;
                        //Snap Z.
                        if (EnumContains.AxesContains(_snapRotation, Axes.Z))
                            startEuler.z = targetEuler.z;

                        //Rebuild into quaternion.
                        t.SetRotation(useLocalSpace, Quaternion.Euler(startEuler));
                    }
                }
            }

            /* Scale.
             * Only snap scale if not Platform Target
             * as platform Target doesn't need scale. */
            if (t != Platform.Target && snapAll || EnumContains.SyncPropertiesContains(sp, SyncProperties.Scale))
            {
                //If to snap all.
                if (snapAll || SnapAll(_snapScale))
                {
                    t.SetScale(scale);
                }
                //Snap some or none.
                else
                {
                    //Snap X.
                    if (EnumContains.AxesContains(_snapScale, Axes.X))
                        t.SetScale(new Vector3(scale.x, t.GetScale().y, t.GetScale().z));
                    //Snap Y.
                    if (EnumContains.AxesContains(_snapScale, Axes.Y))
                        t.SetPosition(UseLocalSpace, new Vector3(t.GetScale().x, scale.y, t.GetScale().z));
                    //Snap Z.
                    if (EnumContains.AxesContains(_snapScale, Axes.Z))
                        t.SetPosition(UseLocalSpace, new Vector3(t.GetScale().x, t.GetScale().y, scale.z));
                }
            }
        }

        /// <summary>
        /// Sends SyncData to clients.
        /// </summary>
        /// <param name="data"></param>
        [ClientRpc(channel = 0, excludeOwner = true)]
        private void RpcSendSyncDataReliableExcludeOwner(TransformSyncData data)
        {
            ServerDataReceived(data);
        }
        /// <summary>
        /// Sends SyncData to clients.
        /// </summary>
        /// <param name="data"></param>
        [ClientRpc(channel = 0)]
        private void RpcSendSyncDataReliable(TransformSyncData data)
        {
            ServerDataReceived(data);
        }
        /// <summary>
        ///         /// <summary>
        /// Sends SyncData to clients.
        /// </summary>
        /// <param name="data"></param>
        [ClientRpc(channel = 1, excludeOwner = true)]
        private void RpcSendSyncDataUnreliableExcludeOwner(TransformSyncData data)
        {
            ServerDataReceived(data);
        }
        /// Sends SyncData to clients.
        /// </summary>
        /// <param name="data"></param>
        [ClientRpc(channel = 1)]
        private void RpcSendSyncDataUnreliable(TransformSyncData data)
        {
            ServerDataReceived(data);
        }

        /// <summary>
        /// Called on clients when server data is received.
        /// </summary>
        /// <param name="data"></param>
        [Client]
        private void ServerDataReceived(TransformSyncData data)
        {
            //If client host exit method.
            if (NetworkIsServer())
                return;

            //If owner of object.
            if (NetworkHasAuthority())
            {
                //Client authoritative, already in sync.
                if (_clientAuthoritative)
                    return;
                //Not client authoritative, but also not sync to owner.
                if (!_clientAuthoritative && !_synchronizeToOwner)
                    return;
            }

            if (OutOfSequence(data, ref _lastServerReceivedSequenceId))
                return;

            //Fill in missing data for properties that werent included in send.
            FillMissingData(data, _targetData);

            /* If platform is valid then set the target transform
            * to values received from the client. */
            bool platformValid = UpdatePlatform(data.PlatformNetId);

            ExtrapolationData extrapolation = null;
            MoveRateData moveRates;

            //If teleporting set move rates to be instantaneous.
            if (ShouldTeleport(data, platformValid))
            {
                moveRates = SetInstantMoveRates();
            }
            //If not teleporting calculate extrapolation and move rates.
            else
            {
                extrapolation = SetExtrapolation(data, _targetData, platformValid);
                moveRates = SetMoveRates(data, platformValid);
            }

            ApplyTransformSnapping(data, false, platformValid);
            _targetData = new TargetSyncData(data, moveRates, extrapolation);
        }

        /// <summary>
        /// Modifies values within goalData based on what data was included in the packet.
        /// For example, if rotation was not included in the packet then the last datas rotation will be used, or transforms current rotation if there is no previous packet.
        /// </summary>
        private void FillMissingData(TransformSyncData data, TargetSyncData targetSyncData)
        {
            SyncProperties sp = (SyncProperties)data.SyncProperties;
            /* Begin by setting goal data using what has been serialized
            * via the writer. */
            //Position wasn't included.
            if (!EnumContains.SyncPropertiesContains(sp, SyncProperties.Position))
            {
                if (targetSyncData == null)
                    data.Position = TargetTransform.GetPosition(UseLocalSpace);
                else
                    data.Position = targetSyncData.GoalData.Position;
            }
            //Rotation wasn't included.
            if (!EnumContains.SyncPropertiesContains(sp, SyncProperties.Rotation))
            {
                if (targetSyncData == null)
                    data.Rotation = TargetTransform.GetRotation(UseLocalSpace);
                else
                    data.Rotation = targetSyncData.GoalData.Rotation;
            }
            //Scale wasn't included.
            if (!EnumContains.SyncPropertiesContains(sp, SyncProperties.Scale))
            {
                if (targetSyncData == null)
                    data.Scale = TargetTransform.GetScale();
                else
                    data.Scale = targetSyncData.GoalData.Scale;
            }

            /* Platform data will always be included every packet
             * if a platform is present. */
        }

        #region Network specific Support.
        /// <summary>
        /// Returns true if object has an owner.
        /// </summary>
        /// <returns></returns>
        protected bool NetworkHasOwner()
        {
#if MIRROR
            return (base.connectionToClient != null);
#elif MIRRORNG
            return (base.ConnectionToClient != null);
#endif
        }
        /// <summary>
        /// Returns if current client has authority.
        /// </summary>
        /// <returns></returns>
        protected bool NetworkHasAuthority()
        {
#if MIRROR
            return base.hasAuthority;
#elif MIRRORNG
            return base.HasAuthority;
#endif
        }
        /// <summary>
        /// Returns if is server.
        /// </summary>
        /// <returns></returns>
        protected bool NetworkIsServer()
        {
#if MIRROR
            return base.isServer;
#elif MIRRORNG
            return base.IsServer;
#endif

        }
        protected bool NetworkIsServerOnly()
        {
#if MIRROR
            return base.isServerOnly;
#elif MIRRORNG
            return base.IsServerOnly;
#endif

        }
        /// <summary>
        /// Returns if is client.
        /// </summary>
        /// <returns></returns>
        protected bool NetworkIsClient()
        {
#if MIRROR
            return base.isClient;
#elif MIRRORNG
            return base.IsClient;
#endif

        }
        #endregion

        #region Editor.
        private void OnValidate()
        {
            SetTeleportThresholdSquared();
        }
        #endregion
    }
}

