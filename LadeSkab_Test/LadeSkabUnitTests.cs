﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NSubstitute;
using LadeSkab;

namespace LadeSkab_Test
{
    [TestFixture]
    public class DoorUnitTests
    {
        private Door _uut;
        private DoorEventArgs _receivedEventArgs;

        [SetUp]
        public void Setup()
        {
            _receivedEventArgs = null;
            _uut = new Door();

            _uut.DoorStatusChanged += (o, args) => { _receivedEventArgs = args; };
        }

        [Test]
        public void OnOpenDoor_DoorOpenedFromClosed_EventFiredWithOpen()
        {
            _uut.OnDoorClose();
            _receivedEventArgs = null;
            _uut.OnDoorOpen();
            if (_receivedEventArgs != null)
                Assert.That(_receivedEventArgs.DoorStatus, Is.EqualTo(DoorEventArgs.DoorState.Open));
        }

        [Test]
        public void OnCloseDoor_DoorClosedFromOpen_EventFiredWithClose()
        {
            _uut.OnDoorOpen();
            _receivedEventArgs = null;
            _uut.OnDoorClose();
            if (_receivedEventArgs != null)
                Assert.That(_receivedEventArgs.DoorStatus, Is.EqualTo(DoorEventArgs.DoorState.Closed));
        }

        [Test]
        public void OnCloseDoor_DoorClosedFromClosed_NoEventFired()
        {
            _uut.OnDoorClose();
            _uut.UnlockDoor();
            _receivedEventArgs = null;
            _uut.OnDoorClose();
            Assert.That(_receivedEventArgs, Is.Null);
        }

        [Test]
        public void OnOpenDoor_DoorOpenFromOpen_NoEventFired()
        {
            _uut.OnDoorOpen();
            _uut.UnlockDoor();
            _receivedEventArgs = null;
            _uut.OnDoorOpen();
            Assert.That(_receivedEventArgs, Is.Null);
        }

        [Test]
        public void OnOpenDoor_DoorOpenOnLockedDoor_NoEventFired()
        {
            _uut.OnDoorClose();
            _uut.LockDoor();
            _receivedEventArgs = null;
            _uut.OnDoorOpen();
            Assert.That(_receivedEventArgs, Is.Null);
        }
    }

    [TestFixture]
    public class StationControlUnitTests
    {
        //[SetUp]
        //public class 



    }

    [TestFixture]
    public class RFIDReaderUnitTests
    {
        private RFIDReader _uut;
        private Nullable<int> _receivedEventArgs;

        [SetUp]
        public void Setup()
        {
            _receivedEventArgs = null;

            _uut = new RFIDReader();

            _uut.IdDetectedEvent += (o, args) => { _receivedEventArgs = args; };
        }

        [Test]
        public void OnIdRead_ValidIdRead_EventFired()
        {
            int validId = 123;
            _uut.OnIdRead(validId);

            Assert.That(_receivedEventArgs, Is.Not.Null);
        }

        [Test]
        public void OnIdRead_ValidIdRead_CorrectValidIdReceived()
        {
            int validId = 123;
            _uut.OnIdRead(validId);

            Assert.That(_receivedEventArgs, Is.EqualTo(validId));
        }

        [TestCase(-100)]
        [TestCase(0)]
        public void OnIdRead_DifferentInvalidIdReads_NoEventFired(int invalidId)
        {
            _uut.OnIdRead(invalidId);

            Assert.That(_receivedEventArgs, Is.Null); // Null => No event fired
        }
    }





}
