using Mirror.RemoteCalls;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirror.Tests
{
    class EmptyBehaviour : NetworkBehaviour { }

    class SyncVarNetworkIdentityEqualExposedBehaviour : NetworkBehaviour
    {
        public bool SyncVarNetworkIdentityEqualExposed(NetworkIdentity newNetworkIdentity, uint netIdField)
        {
            return SyncVarNetworkIdentityEqual(newNetworkIdentity, netIdField);
        }
    }

    // we need to inherit from networkbehaviour to test protected functions
    public class NetworkBehaviourSendCommandInternalComponent : NetworkBehaviour
    {
        // counter to make sure that it's called exactly once
        public int called;

        // weaver generates this from [Command]
        // but for tests we need to add it manually
        public static void CommandGenerated(NetworkBehaviour comp, NetworkReader reader, NetworkConnection senderConnection)
        {
            ++((NetworkBehaviourSendCommandInternalComponent)comp).called;
        }

        // SendCommandInternal is protected. let's expose it so we can test it.
        public void CallSendCommandInternal()
        {
            SendCommandInternal(GetType(), nameof(CommandGenerated), new NetworkWriter(1024), 0);
        }
    }

    // we need to inherit from networkbehaviour to test protected functions
    public class NetworkBehaviourSendRPCInternalComponent : NetworkBehaviour
    {
        // counter to make sure that it's called exactly once
        public int called;

        // weaver generates this from [ClientRpc]
        // but for tests we need to add it manually
        public static void RPCGenerated(NetworkBehaviour comp, NetworkReader reader, NetworkConnection senderConnection)
        {
            ++((NetworkBehaviourSendRPCInternalComponent)comp).called;
        }

        // SendCommandInternal is protected. let's expose it so we can test it.
        public void CallSendRPCInternal()
        {
            SendRPCInternal(GetType(), nameof(RPCGenerated), new NetworkWriter(1024), 0, false);
        }
    }

    // we need to inherit from networkbehaviour to test protected functions
    public class NetworkBehaviourSendTargetRPCInternalComponent : NetworkBehaviour
    {
        // counter to make sure that it's called exactly once
        public int called;

        // weaver generates this from [TargetRpc]
        // but for tests we need to add it manually
        public static void TargetRPCGenerated(NetworkBehaviour comp, NetworkReader reader, NetworkConnection senderConnection)
        {
            ++((NetworkBehaviourSendTargetRPCInternalComponent)comp).called;
        }

        // SendCommandInternal is protected. let's expose it so we can test it.
        public void CallSendTargetRPCInternal(NetworkConnection conn)
        {
            SendTargetRPCInternal(conn, GetType(), nameof(TargetRPCGenerated), new NetworkWriter(1024), 0);
        }
    }

    // we need to inherit from networkbehaviour to test protected functions
    public class NetworkBehaviourDelegateComponent : NetworkBehaviour
    {
        public static void Delegate(NetworkBehaviour comp, NetworkReader reader, NetworkConnection senderConnection) { }
        public static void Delegate2(NetworkBehaviour comp, NetworkReader reader, NetworkConnection senderConnection) { }
    }

    // we need to inherit from networkbehaviour to test protected functions
    public class NetworkBehaviourSetSyncVarNetworkIdentityComponent : NetworkBehaviour
    {
        //[SyncVar]
        public NetworkIdentity test;
        // usually generated by weaver
        public uint testNetId;

        // SetSyncVarNetworkIdentity wrapper to expose it
        public void SetSyncVarNetworkIdentityExposed(NetworkIdentity newNetworkIdentity, ulong dirtyBit)
        {
            SetSyncVarNetworkIdentity(newNetworkIdentity, ref test, dirtyBit, ref testNetId);
        }
    }

    // we need to inherit from networkbehaviour to test protected functions
    public class NetworkBehaviourGetSyncVarNetworkIdentityComponent : NetworkBehaviour
    {
        //[SyncVar]
        public NetworkIdentity test;
        // usually generated by weaver
        public uint testNetId;

        // SetSyncVarNetworkIdentity wrapper to expose it
        public NetworkIdentity GetSyncVarNetworkIdentityExposed()
        {
            return GetSyncVarNetworkIdentity(testNetId, ref test);
        }
    }

    // we need to inherit from networkbehaviour to test protected functions
    public class NetworkBehaviourInitSyncObjectExposed : NetworkBehaviour
    {
        public void InitSyncObjectExposed(SyncObject obj)
        {
            InitSyncObject(obj);
        }
    }

    // we need to inherit from networkbehaviour to test protected functions
    public class OnNetworkDestroyComponent : NetworkBehaviour
    {
        public int called;
        public override void OnStopClient()
        {
            ++called;
        }
    }

    // we need to inherit from networkbehaviour to test protected functions
    public class OnStartClientComponent : NetworkBehaviour
    {
        public int called;
        public override void OnStartClient()
        {
            ++called;
        }
    }

    // we need to inherit from networkbehaviour to test protected functions
    public class OnStartLocalPlayerComponent : NetworkBehaviour
    {
        public int called;
        public override void OnStartLocalPlayer()
        {
            ++called;
        }
    }

    public class NetworkBehaviourTests
    {
        GameObject gameObject;
        NetworkIdentity identity;
        // useful in most tests, but not necessarily all tests
        EmptyBehaviour emptyBehaviour;

        [SetUp]
        public void SetUp()
        {
            gameObject = new GameObject();
            identity = gameObject.AddComponent<NetworkIdentity>();

            // add a behaviour for testing
            emptyBehaviour = gameObject.AddComponent<EmptyBehaviour>();
        }

        [TearDown]
        public void TearDown()
        {
            // set isServer is false. otherwise Destroy instead of
            // DestroyImmediate is called internally, giving an error in Editor
            identity.isServer = false;
            GameObject.DestroyImmediate(gameObject);

            NetworkIdentity.spawned.Clear();
        }

        [Test]
        public void IsServerOnly()
        {
            // call OnStartServer so isServer is true
            identity.OnStartServer();
            Assert.That(identity.isServer, Is.True);

            // isServerOnly should be true when isServer = true && isClient = false
            Assert.That(emptyBehaviour.isServer, Is.True);
            Assert.That(emptyBehaviour.isClient, Is.False);
            Assert.That(emptyBehaviour.isServerOnly, Is.True);
        }

        [Test]
        public void IsClientOnly()
        {
            // isClientOnly should be true when isServer = false && isClient = true
            identity.isClient = true;
            Assert.That(emptyBehaviour.isServer, Is.False);
            Assert.That(emptyBehaviour.isClient, Is.True);
            Assert.That(emptyBehaviour.isClientOnly, Is.True);
        }

        [Test]
        public void HasNoAuthorityByDefault()
        {
            // no authority by default
            Assert.That(emptyBehaviour.hasAuthority, Is.False);
        }

        [Test]
        public void HasIdentitysNetId()
        {
            identity.netId = 42;
            Assert.That(emptyBehaviour.netId, Is.EqualTo(42));
        }

        [Test]
        public void ComponentIndex()
        {
            // add one extra component
            EmptyBehaviour extra = gameObject.AddComponent<EmptyBehaviour>();

            // original one is first networkbehaviour, so index is 0
            Assert.That(emptyBehaviour.ComponentIndex, Is.EqualTo(0));

            // extra one is second networkbehaviour, so index is 1
            Assert.That(extra.ComponentIndex, Is.EqualTo(1));
        }

        [Test]
        public void RegisterDelegateDoesntOverwrite()
        {
            // registerdelegate is protected, but we can use
            // RegisterCommandDelegate which calls RegisterDelegate
            int registeredHash1 = RemoteCallHelper.RegisterDelegate(
                typeof(NetworkBehaviourDelegateComponent),
                nameof(NetworkBehaviourDelegateComponent.Delegate),
                MirrorInvokeType.Command,
                NetworkBehaviourDelegateComponent.Delegate,
                false);

            // registering the exact same one should be fine. it should simply
            // do nothing.
            int registeredHash2 = RemoteCallHelper.RegisterDelegate(
                typeof(NetworkBehaviourDelegateComponent),
                nameof(NetworkBehaviourDelegateComponent.Delegate),
                MirrorInvokeType.Command,
                NetworkBehaviourDelegateComponent.Delegate,
                false);
            // registering the same name with a different callback shouldn't
            // work
            LogAssert.Expect(LogType.Error, "Function " + typeof(NetworkBehaviourDelegateComponent) + "." + nameof(NetworkBehaviourDelegateComponent.Delegate) + " and " + typeof(NetworkBehaviourDelegateComponent) + "." + nameof(NetworkBehaviourDelegateComponent.Delegate2) + " have the same hash.  Please rename one of them");
            int registeredHash3 = RemoteCallHelper.RegisterDelegate(
                typeof(NetworkBehaviourDelegateComponent),
                nameof(NetworkBehaviourDelegateComponent.Delegate),
                MirrorInvokeType.Command,
                NetworkBehaviourDelegateComponent.Delegate2,
                false);

            // clean up
            RemoteCallHelper.RemoveDelegate(registeredHash1);
            RemoteCallHelper.RemoveDelegate(registeredHash2);
            RemoteCallHelper.RemoveDelegate(registeredHash3);
        }

        [Test]
        public void GetDelegate()
        {
            // registerdelegate is protected, but we can use
            // RegisterCommandDelegate which calls RegisterDelegate
            int registeredHash = RemoteCallHelper.RegisterDelegate(
                typeof(NetworkBehaviourDelegateComponent),
                nameof(NetworkBehaviourDelegateComponent.Delegate),
                MirrorInvokeType.Command,
                NetworkBehaviourDelegateComponent.Delegate,
                false);

            // get handler
            int cmdHash = RemoteCallHelper.GetMethodHash(typeof(NetworkBehaviourDelegateComponent), nameof(NetworkBehaviourDelegateComponent.Delegate));
            CmdDelegate func = RemoteCallHelper.GetDelegate(cmdHash);
            CmdDelegate expected = NetworkBehaviourDelegateComponent.Delegate;
            Assert.That(func, Is.EqualTo(expected));

            // invalid hash should return null handler
            CmdDelegate funcNull = RemoteCallHelper.GetDelegate(1234);
            Assert.That(funcNull, Is.Null);

            // clean up
            RemoteCallHelper.RemoveDelegate(registeredHash);
        }

        // NOTE: SyncVarNetworkIdentityEqual should be static later
        [Test]
        public void SyncVarNetworkIdentityEqualZeroNetIdNullIsTrue()
        {
            // null and identity.netid==0 returns true (=equal)
            //
            // later we should reevaluate if this is so smart or not. might be
            // better to return false here.
            // => we possibly return false so that resync doesn't happen when
            //    GO disappears? or not?
            SyncVarNetworkIdentityEqualExposedBehaviour comp = gameObject.AddComponent<SyncVarNetworkIdentityEqualExposedBehaviour>();
            bool result = comp.SyncVarNetworkIdentityEqualExposed(null, identity.netId);
            Assert.That(result, Is.True);
        }

        // NOTE: SyncVarNetworkIdentityEqual should be static later
        [Test]
        public void SyncVarNetworkIdentityEqualNull()
        {
            // our identity should have a netid for comparing
            identity.netId = 42;

            // null should return false
            SyncVarNetworkIdentityEqualExposedBehaviour comp = gameObject.AddComponent<SyncVarNetworkIdentityEqualExposedBehaviour>();
            bool result = comp.SyncVarNetworkIdentityEqualExposed(null, identity.netId);
            Assert.That(result, Is.False);
        }

        // NOTE: SyncVarNetworkIdentityEqual should be static later
        [Test]
        public void SyncVarNetworkIdentityEqualValidIdentityWithDifferentNetId()
        {
            // our identity should have a netid for comparing
            identity.netId = 42;

            // gameobject with valid networkidentity and netid that is different
            GameObject go = new GameObject();
            NetworkIdentity ni = go.AddComponent<NetworkIdentity>();
            SyncVarNetworkIdentityEqualExposedBehaviour comp = go.AddComponent<SyncVarNetworkIdentityEqualExposedBehaviour>();
            ni.netId = 43;
            bool result = comp.SyncVarNetworkIdentityEqualExposed(ni, identity.netId);
            Assert.That(result, Is.False);

            // clean up
            GameObject.DestroyImmediate(go);
        }

        // NOTE: SyncVarNetworkIdentityEqual should be static later
        [Test]
        public void SyncVarNetworkIdentityEqualValidIdentityWithSameNetId()
        {
            // our identity should have a netid for comparing
            identity.netId = 42;

            // gameobject with valid networkidentity and netid that is different
            GameObject go = new GameObject();
            NetworkIdentity ni = go.AddComponent<NetworkIdentity>();
            SyncVarNetworkIdentityEqualExposedBehaviour comp = go.AddComponent<SyncVarNetworkIdentityEqualExposedBehaviour>();
            ni.netId = 42;
            bool result = comp.SyncVarNetworkIdentityEqualExposed(ni, identity.netId);
            Assert.That(result, Is.True);

            // clean up
            GameObject.DestroyImmediate(go);
        }

        // NOTE: SyncVarNetworkIdentityEqual should be static later
        [Test]
        public void SyncVarNetworkIdentityEqualUnspawnedIdentity()
        {
            // our identity should have a netid for comparing
            identity.netId = 42;

            // gameobject with valid networkidentity and 0 netid that is unspawned
            GameObject go = new GameObject();
            NetworkIdentity ni = go.AddComponent<NetworkIdentity>();
            SyncVarNetworkIdentityEqualExposedBehaviour comp = go.AddComponent<SyncVarNetworkIdentityEqualExposedBehaviour>();
            LogAssert.Expect(LogType.Warning, "SetSyncVarNetworkIdentity NetworkIdentity " + ni + " has a zero netId. Maybe it is not spawned yet?");
            bool result = comp.SyncVarNetworkIdentityEqualExposed(ni, identity.netId);
            Assert.That(result, Is.False);

            // clean up
            GameObject.DestroyImmediate(go);
        }

        // NOTE: SyncVarNetworkIdentityEqual should be static later
        [Test]
        public void SyncVarNetworkIdentityEqualUnspawnedIdentityZeroNetIdIsTrue()
        {
            // unspawned go and identity.netid==0 returns true (=equal)
            GameObject go = new GameObject();
            NetworkIdentity ni = go.AddComponent<NetworkIdentity>();
            SyncVarNetworkIdentityEqualExposedBehaviour comp = go.AddComponent<SyncVarNetworkIdentityEqualExposedBehaviour>();
            LogAssert.Expect(LogType.Warning, "SetSyncVarNetworkIdentity NetworkIdentity " + ni + " has a zero netId. Maybe it is not spawned yet?");
            bool result = comp.SyncVarNetworkIdentityEqualExposed(ni, identity.netId);
            Assert.That(result, Is.True);

            // clean up
            GameObject.DestroyImmediate(go);
        }

        [Test]
        public void SetSyncVarNetworkIdentityWithValidObject()
        {
            // add test component
            NetworkBehaviourSetSyncVarNetworkIdentityComponent comp = gameObject.AddComponent<NetworkBehaviourSetSyncVarNetworkIdentityComponent>();
            // for isDirty check
            comp.syncInterval = 0;

            // create a valid GameObject with networkidentity and netid
            GameObject go = new GameObject();
            NetworkIdentity ni = go.AddComponent<NetworkIdentity>();
            ni.netId = 43;

            // set the NetworkIdentity SyncVar
            Assert.That(comp.IsDirty(), Is.False);
            comp.SetSyncVarNetworkIdentityExposed(ni, 1ul);
            Assert.That(comp.test, Is.EqualTo(ni));
            Assert.That(comp.testNetId, Is.EqualTo(ni.netId));
            Assert.That(comp.IsDirty(), Is.True);

            // clean up
            GameObject.DestroyImmediate(go);
        }

        [Test]
        public void SetSyncVarNetworkIdentityNull()
        {
            // add test component
            NetworkBehaviourSetSyncVarNetworkIdentityComponent comp = gameObject.AddComponent<NetworkBehaviourSetSyncVarNetworkIdentityComponent>();
            // for isDirty check
            comp.syncInterval = 0;

            // set some existing NI+netId first to check if it is going to be
            // overwritten
            GameObject go = new GameObject();
            NetworkIdentity ni = go.AddComponent<NetworkIdentity>();
            comp.test = ni;
            comp.testNetId = 43;

            // set the NetworkIdentity SyncVar to null
            Assert.That(comp.IsDirty(), Is.False);
            comp.SetSyncVarNetworkIdentityExposed(null, 1ul);
            Assert.That(comp.test, Is.EqualTo(null));
            Assert.That(comp.testNetId, Is.EqualTo(0));
            Assert.That(comp.IsDirty(), Is.True);

            // clean up
            GameObject.DestroyImmediate(go);
        }

        [Test]
        public void SetSyncVarNetworkIdentityZeroNetId()
        {
            // add test component
            NetworkBehaviourSetSyncVarNetworkIdentityComponent comp = gameObject.AddComponent<NetworkBehaviourSetSyncVarNetworkIdentityComponent>();
            // for isDirty check
            comp.syncInterval = 0;

            // set some existing NI+netId first to check if it is going to be
            // overwritten
            GameObject go = new GameObject();
            NetworkIdentity ni = go.AddComponent<NetworkIdentity>();
            comp.test = ni;
            comp.testNetId = 43;

            // create test GO with networkidentity and zero netid
            GameObject test = new GameObject();
            NetworkIdentity testNi = test.AddComponent<NetworkIdentity>();
            Assert.That(testNi.netId, Is.EqualTo(0));

            // set the NetworkIdentity SyncVar to 'test' GO with zero netId.
            // -> the way it currently works is that it sets netId to 0, but
            //    it DOES set gameObjectField to the GameObject without the
            //    NetworkIdentity, instead of setting it to null because it's
            //    seemingly invalid.
            // -> there might be a deeper reason why UNET did that. perhaps for
            //    cases where we assign a GameObject before the network was
            //    fully started and has no netId or networkidentity yet etc.
            // => it works, so let's keep it for now
            Assert.That(comp.IsDirty(), Is.False);
            LogAssert.Expect(LogType.Warning, "SetSyncVarNetworkIdentity NetworkIdentity " + testNi + " has a zero netId. Maybe it is not spawned yet?");
            comp.SetSyncVarNetworkIdentityExposed(testNi, 1ul);
            Assert.That(comp.test, Is.EqualTo(testNi));
            Assert.That(comp.testNetId, Is.EqualTo(0));
            Assert.That(comp.IsDirty(), Is.True);

            // clean up
            GameObject.DestroyImmediate(test);
            GameObject.DestroyImmediate(go);
        }

        [Test]
        public void GetSyncVarNetworkIdentityOnServer()
        {
            // call OnStartServer so isServer is true
            identity.OnStartServer();
            Assert.That(identity.isServer, Is.True);

            // add test component
            NetworkBehaviourGetSyncVarNetworkIdentityComponent comp = gameObject.AddComponent<NetworkBehaviourGetSyncVarNetworkIdentityComponent>();
            // for isDirty check
            comp.syncInterval = 0;

            // create a syncable GameObject
            GameObject go = new GameObject();
            NetworkIdentity ni = go.AddComponent<NetworkIdentity>();
            ni.netId = identity.netId + 1;

            // assign it in the component
            comp.test = ni;
            comp.testNetId = ni.netId;

            // get it on the server. should simply return the field instead of
            // doing any netId lookup like on the client
            NetworkIdentity result = comp.GetSyncVarNetworkIdentityExposed();
            Assert.That(result, Is.EqualTo(ni));

            // clean up
            GameObject.DestroyImmediate(go);
        }

        [Test]
        public void GetSyncVarNetworkIdentityOnServerNull()
        {
            // call OnStartServer so isServer is true
            identity.OnStartServer();
            Assert.That(identity.isServer, Is.True);

            // add test component
            NetworkBehaviourGetSyncVarNetworkIdentityComponent comp = gameObject.AddComponent<NetworkBehaviourGetSyncVarNetworkIdentityComponent>();
            // for isDirty check
            comp.syncInterval = 0;

            // get it on the server. null should work fine.
            NetworkIdentity result = comp.GetSyncVarNetworkIdentityExposed();
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetSyncVarNetworkIdentityOnClient()
        {
            // are we on client and not on server?
            identity.isClient = true;
            Assert.That(identity.isServer, Is.False);

            // add test component
            NetworkBehaviourGetSyncVarNetworkIdentityComponent comp = gameObject.AddComponent<NetworkBehaviourGetSyncVarNetworkIdentityComponent>();
            // for isDirty check
            comp.syncInterval = 0;

            // create a syncable GameObject
            GameObject go = new GameObject();
            NetworkIdentity ni = go.AddComponent<NetworkIdentity>();
            ni.netId = 43;

            // register in spawned dict because clients should look it up via
            // netId
            NetworkIdentity.spawned[ni.netId] = ni;

            // assign ONLY netId in the component. assume that GameObject was
            // assigned earlier but client walked so far out of range that it
            // was despawned on the client. so it's forced to do the netId look-
            // up.
            Assert.That(comp.test, Is.Null);
            comp.testNetId = ni.netId;

            // get it on the client. should look up netId in spawned
            NetworkIdentity result = comp.GetSyncVarNetworkIdentityExposed();
            Assert.That(result, Is.EqualTo(ni));

            // clean up
            NetworkServer.Shutdown();
            Transport.activeTransport = null;
            GameObject.DestroyImmediate(go);
        }

        [Test]
        public void GetSyncVarNetworkIdentityOnClientNull()
        {
            // are we on client and not on server?
            identity.isClient = true;
            Assert.That(identity.isServer, Is.False);

            // add test component
            NetworkBehaviourGetSyncVarNetworkIdentityComponent comp = gameObject.AddComponent<NetworkBehaviourGetSyncVarNetworkIdentityComponent>();
            // for isDirty check
            comp.syncInterval = 0;

            // get it on the client. null should be supported.
            NetworkIdentity result = comp.GetSyncVarNetworkIdentityExposed();
            Assert.That(result, Is.Null);

            // clean up
            NetworkServer.Shutdown();
            Transport.activeTransport = null;
        }

        [Test]
        public void ClearAllDirtyBitsClearsSyncVarDirtyBits()
        {
            // set syncinterval so dirtybit works fine
            emptyBehaviour.syncInterval = 0;
            Assert.That(emptyBehaviour.IsDirty(), Is.False);

            // set one syncvar dirty bit
            emptyBehaviour.SetDirtyBit(1);
            Assert.That(emptyBehaviour.IsDirty(), Is.True);

            // clear it
            emptyBehaviour.ClearAllDirtyBits();
            Assert.That(emptyBehaviour.IsDirty(), Is.False);
        }

        [Test]
        public void ClearAllDirtyBitsClearsSyncObjectsDirtyBits()
        {
            // we need a component that exposes InitSyncObject so we can set
            // sync objects dirty
            NetworkBehaviourInitSyncObjectExposed comp = gameObject.AddComponent<NetworkBehaviourInitSyncObjectExposed>();

            // set syncinterval so dirtybit works fine
            comp.syncInterval = 0;
            Assert.That(comp.IsDirty(), Is.False);

            // create a synclist and dirty it
            SyncList<int> obj = new SyncList<int>();
            obj.Add(42);
            Assert.That(obj.IsDirty, Is.True);

            // add it
            comp.InitSyncObjectExposed(obj);
            Assert.That(comp.IsDirty, Is.True);

            // clear bits should clear synclist bits too
            comp.ClearAllDirtyBits();
            Assert.That(comp.IsDirty, Is.False);
            Assert.That(obj.IsDirty, Is.False);
        }

        [Test]
        public void DirtyObjectBits()
        {
            // we need a component that exposes InitSyncObject so we can set
            // sync objects dirty
            NetworkBehaviourInitSyncObjectExposed comp = gameObject.AddComponent<NetworkBehaviourInitSyncObjectExposed>();

            // not dirty by default
            Assert.That(comp.DirtyObjectBits(), Is.EqualTo(0b0));

            // add a dirty synclist
            SyncList<int> dirtyList = new SyncList<int>();
            dirtyList.Add(42);
            Assert.That(dirtyList.IsDirty, Is.True);
            comp.InitSyncObjectExposed(dirtyList);

            // add a clean synclist
            SyncList<int> cleanList = new SyncList<int>();
            Assert.That(cleanList.IsDirty, Is.False);
            comp.InitSyncObjectExposed(cleanList);

            // get bits - only first one should be dirty
            Assert.That(comp.DirtyObjectBits(), Is.EqualTo(0b1));

            // set second one dirty. now we should have two dirty bits
            cleanList.Add(43);
            Assert.That(comp.DirtyObjectBits(), Is.EqualTo(0b11));
        }

        [Test]
        public void SerializeAndDeserializeObjectsAll()
        {
            // we need a component that exposes InitSyncObject so we can add some
            NetworkBehaviourInitSyncObjectExposed comp = gameObject.AddComponent<NetworkBehaviourInitSyncObjectExposed>();

            // add a synclist
            SyncList<int> list = new SyncList<int>();
            list.Add(42);
            list.Add(43);
            Assert.That(list.IsDirty, Is.True);
            comp.InitSyncObjectExposed(list);

            // serialize it
            NetworkWriter writer = new NetworkWriter(1024);
            comp.SerializeObjectsAll(writer);

            // clear original list
            list.Clear();
            Assert.That(list.Count, Is.EqualTo(0));

            // deserialize it
            NetworkReader reader = new NetworkReader(writer.ToArraySegment());
            comp.DeSerializeObjectsAll(reader);
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0], Is.EqualTo(42));
            Assert.That(list[1], Is.EqualTo(43));
        }

        [Test]
        public void SerializeAndDeserializeObjectsDelta()
        {
            // we need a component that exposes InitSyncObject so we can add some
            NetworkBehaviourInitSyncObjectExposed comp = gameObject.AddComponent<NetworkBehaviourInitSyncObjectExposed>();

            // add a synclist
            SyncList<int> list = new SyncList<int>();
            list.Add(42);
            list.Add(43);
            Assert.That(list.IsDirty, Is.True);
            comp.InitSyncObjectExposed(list);

            // serialize it
            NetworkWriter writer = new NetworkWriter(1024);
            comp.SerializeObjectsDelta(writer);

            // clear original list
            list.Clear();
            Assert.That(list.Count, Is.EqualTo(0));

            // deserialize it
            NetworkReader reader = new NetworkReader(writer.ToArraySegment());
            comp.DeSerializeObjectsDelta(reader);
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0], Is.EqualTo(42));
            Assert.That(list[1], Is.EqualTo(43));
        }

        [Test]
        public void OnNetworkDestroy()
        {
            // add test component
            OnNetworkDestroyComponent comp = gameObject.AddComponent<OnNetworkDestroyComponent>();
            Assert.That(comp.called, Is.EqualTo(0));

            // call identity OnNetworkDestroy
            identity.OnStopClient();

            // should have been forwarded to behaviours
            Assert.That(comp.called, Is.EqualTo(1));
        }

        [Test]
        public void OnStartClient()
        {
            // add test component
            OnStartClientComponent comp = gameObject.AddComponent<OnStartClientComponent>();
            Assert.That(comp.called, Is.EqualTo(0));

            // call identity OnNetworkDestroy
            identity.OnStartClient();

            // should have been forwarded to behaviours
            Assert.That(comp.called, Is.EqualTo(1));
        }

        [Test]
        public void OnStartLocalPlayer()
        {
            // add test component
            OnStartLocalPlayerComponent comp = gameObject.AddComponent<OnStartLocalPlayerComponent>();
            Assert.That(comp.called, Is.EqualTo(0));

            // call identity OnNetworkDestroy
            identity.OnStartLocalPlayer();

            // should have been forwarded to behaviours
            Assert.That(comp.called, Is.EqualTo(1));
        }
    }

    // we need to inherit from networkbehaviour to test protected functions
    public class NetworkBehaviourInitSyncObjectTester : NetworkBehaviour
    {
        [Test]
        public void InitSyncObject()
        {
            SyncObject syncObject = new SyncList<bool>();
            InitSyncObject(syncObject);
            Assert.That(syncObjects.Count, Is.EqualTo(1));
            Assert.That(syncObjects[0], Is.EqualTo(syncObject));
        }
    }
}
