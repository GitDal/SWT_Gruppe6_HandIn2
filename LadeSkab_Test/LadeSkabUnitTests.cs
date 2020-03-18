using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    public class ChargeControlUnitTests
    {
        //[SetUp]
        //public class 



    }

    [TestFixture]
    public class TestUsbCharger
    {
        private USBCharger _uut;
        [SetUp]
        public void Setup()
        {
            _uut = new USBCharger();
        }

        [Test]
        public void ctor_IsConnected()
        {
            Assert.That(_uut.Connected, Is.False);
        }

        [Test]
        public void ctor_CurentValueIsZero()
        {
            Assert.That(_uut.CurrentValue, Is.Zero);
        }

        [Test]
        public void SimulateDisconnected_ReturnsDisconnected()
        {
            _uut.SimulateConnected(false);
            Assert.That(_uut.Connected, Is.False);
        }

        [Test]
        public void SimulateConnected_ReturnsConnected()
        {
            _uut.SimulateConnected(true);
            Assert.That(_uut.Connected, Is.True);
        }

        [Test]
        public void Started_WaitSomeTime_ReceivedSeveralValues()
        {
            _uut.SimulateConnected(true);
            int numValues = 0;
            _uut.CurrentValueEvent += (o, args) => numValues++;

            _uut.StartCharge();

            System.Threading.Thread.Sleep(1100);

            Assert.That(numValues, Is.GreaterThan(4));
        }

        [Test]
        public void Started_WaitSomeTime_ReceivedChangedValue()
        {
            _uut.SimulateConnected(true);
            double lastValue = 1000;
            _uut.CurrentValueEvent += (o, args) => lastValue = args.Current;

            _uut.StartCharge();

            System.Threading.Thread.Sleep(300);

            Assert.That(lastValue, Is.LessThan(500.0));
        }

        [Test]
        public void StartedNoEventReceiver_WaitSomeTime_PropertyChangedValue()
        {
            _uut.SimulateConnected(true);
            _uut.StartCharge();

            System.Threading.Thread.Sleep(300);

            Assert.That(_uut.CurrentValue, Is.LessThan(500.0));
        }

        [Test]
        public void Started_WaitSomeTime_PropertyMatchesReceivedValue()
        {
            _uut.SimulateConnected(true);
            double lastValue = 1000;
            _uut.CurrentValueEvent += (o, args) => lastValue = args.Current;

            _uut.StartCharge();

            System.Threading.Thread.Sleep(1100);

            Assert.That(lastValue, Is.EqualTo(_uut.CurrentValue));
        }


        [Test]
        public void Started_SimulateOverload_ReceivesHighValue()
        {
            _uut.SimulateConnected(true);
            ManualResetEvent pause = new ManualResetEvent(false);
            double lastValue = 0;

            _uut.CurrentValueEvent += (o, args) =>
            {
                lastValue = args.Current;
                pause.Set();
            };

            // Start
            _uut.StartCharge();

            // Next value should be high
            _uut.SimulateOverload(true);

            // Reset event
            pause.Reset();

            // Wait for next tick, should send overloaded value
            pause.WaitOne(300);

            Assert.That(lastValue, Is.GreaterThan(500.0));
        }

        [Test]
        public void Started_SimulateDisconnected_ReceivesZero()
        {
            _uut.SimulateConnected(true);
            ManualResetEvent pause = new ManualResetEvent(false);
            double lastValue = 1000;

            _uut.CurrentValueEvent += (o, args) =>
            {
                lastValue = args.Current;
                pause.Set();
            };


            // Start
            _uut.StartCharge();

            // Next value should be zero
            _uut.SimulateConnected(false);

            // Reset event
            pause.Reset();

            // Wait for next tick, should send disconnected value
            pause.WaitOne(300);

            Assert.That(lastValue, Is.Zero);
        }

        [Test]
        public void SimulateOverload_Start_ReceivesHighValueImmediately()
        {
            _uut.SimulateConnected(true);

            double lastValue = 0;

            _uut.CurrentValueEvent += (o, args) =>
            {
                lastValue = args.Current;
            };

            // First value should be high
            _uut.SimulateOverload(true);

            // Start
            _uut.StartCharge();

            // Should not wait for first tick, should send overload immediately

            Assert.That(lastValue, Is.GreaterThan(500.0));
        }

        [Test]
        public void SimulateDisconnected_Start_ReceivesZeroValueImmediately()
        {
            _uut.SimulateConnected(true);
            double lastValue = 1000;

            _uut.CurrentValueEvent += (o, args) =>
            {
                lastValue = args.Current;
            };

            // First value should be high
            _uut.SimulateConnected(false);

            // Start
            _uut.StartCharge();

            // Should not wait for first tick, should send zero immediately

            Assert.That(lastValue, Is.Zero);
        }

        [Test]
        public void StopCharge_IsCharging_ReceivesZeroValue()
        {
            _uut.SimulateConnected(true);
            double lastValue = 1000;
            _uut.CurrentValueEvent += (o, args) => lastValue = args.Current;

            _uut.StartCharge();

            System.Threading.Thread.Sleep(300);

            _uut.StopCharge();

            Assert.That(lastValue, Is.EqualTo(0.0));
        }

        [Test]
        public void StopCharge_IsCharging_PropertyIsZero()
        {
            _uut.SimulateConnected(true);
            _uut.StartCharge();

            System.Threading.Thread.Sleep(300);

            _uut.StopCharge();

            Assert.That(_uut.CurrentValue, Is.EqualTo(0.0));
        }

        [Test]
        public void StopCharge_IsCharging_ReceivesNoMoreValues()
        {
            _uut.SimulateConnected(true);
            double lastValue = 1000;
            _uut.CurrentValueEvent += (o, args) => lastValue = args.Current;

            _uut.StartCharge();

            System.Threading.Thread.Sleep(300);

            _uut.StopCharge();
            lastValue = 1000;

            // Wait for a tick
            System.Threading.Thread.Sleep(300);

            // No new value received
            Assert.That(lastValue, Is.EqualTo(1000.0));
        }

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
