﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Networking;
using NuGet.Frameworks;

namespace Testing.Networking
{
    [TestFixture]
    public class ReceiveQueueListenerTest
    {
        private IQueue _queue;
        private Dictionary<string, INotificationHandler> _notificationHandlers;
        private ReceiveQueueListener _receiveQueueListener;

        private string Message => NetworkingGlobals.GetRandomString();

        [SetUp]
        public void Setup()
        {
            _queue = new Queue();
            _notificationHandlers = new();
            _queue.RegisterModule(Modules.WhiteBoard, Priorities.WhiteBoard);
            _queue.RegisterModule(Modules.ScreenShare, Priorities.ScreenShare);
            _queue.RegisterModule(Modules.File, Priorities.File);
            
            FakeNotificationHandler fakeWhiteBoard = new FakeNotificationHandler();
            FakeNotificationHandler fakeScreenShare = new FakeNotificationHandler();
            FakeNotificationHandler fakeFileShare = new FakeNotificationHandler();

            _notificationHandlers[Modules.WhiteBoard] = fakeWhiteBoard;
            _notificationHandlers[Modules.ScreenShare] = fakeScreenShare;
            _notificationHandlers[Modules.File] = fakeFileShare;

            _receiveQueueListener = new ReceiveQueueListener(_queue, _notificationHandlers);
            _receiveQueueListener.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _receiveQueueListener.Stop();
            _queue = null;
            _receiveQueueListener = null;
            _notificationHandlers = null;
        }

        [Test]
        public void ListenQueue_DequeuingFromQueueAndCallingHandler_ShouldCallAppropriateHandler()
        {
            const string whiteBoardData = "whiteboard";
            const string screenShareData = "screenshare";
            const string fileShareData = "file";
            
            Packet whiteBoardPacket = new Packet{ModuleIdentifier = Modules.WhiteBoard, SerializedData = whiteBoardData};
            Packet screenSharePacket = new Packet{ModuleIdentifier = Modules.ScreenShare, SerializedData = screenShareData};
            Packet fileSharePacket = new Packet{ModuleIdentifier = Modules.File, SerializedData = fileShareData};
            
            _queue.Enqueue(whiteBoardPacket);
            _queue.Enqueue(screenSharePacket);
            _queue.Enqueue(fileSharePacket);
            
            Thread.Sleep(100);
            
            FakeNotificationHandler whiteBoardHandler = (FakeNotificationHandler) _notificationHandlers[Modules.WhiteBoard];
            FakeNotificationHandler screenShareHandler = (FakeNotificationHandler) _notificationHandlers[Modules.ScreenShare];
            FakeNotificationHandler fileShareHandler = (FakeNotificationHandler) _notificationHandlers[Modules.File];
            
            Assert.AreEqual(NotificationEvents.OnDataReceived, screenShareHandler.ReceivedData.Event);
            Assert.AreEqual(screenShareData, screenShareHandler.ReceivedData.Data);
            
            Assert.AreEqual(NotificationEvents.OnDataReceived, whiteBoardHandler.ReceivedData.Event);
            Assert.AreEqual(whiteBoardData, whiteBoardHandler.ReceivedData.Data);
            
            Assert.AreEqual(NotificationEvents.OnDataReceived, fileShareHandler.ReceivedData.Event);
            Assert.AreEqual(fileShareData, fileShareHandler.ReceivedData.Data);
        }
    }
}