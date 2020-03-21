using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Internal;
using NUnit.Framework;
using NSubstitute;
using LadeSkab;
using NSubstitute.Core.Arguments;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework.Internal;
using ILogger = LadeSkab.ILogger;

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
        private StationControl _uut;
        private IDoor _doorSubject;
        private IIdentificationKeyReader<int> _readerSubject;
        private IDisplay _mockDisplay;
        private IChargeControl _mockChargeControl;
        private ILogger _mockLogger;

        [SetUp]
        public void Setup()
        {
            _doorSubject = Substitute.For<IDoor>();
            _readerSubject = Substitute.For<IIdentificationKeyReader<int>>();
            _mockDisplay = Substitute.For<IDisplay>();
            _mockChargeControl = Substitute.For<IChargeControl>();
            _mockLogger = Substitute.For<ILogger>();
            
            StationControl _uut = new StationControl();
            _uut.Door = _doorSubject;
            _uut.Reader = _readerSubject;
            _uut.Display = _mockDisplay;
            _uut.ChargeControl = _mockChargeControl;
            _uut.Logger = _mockLogger;
        }

        #region TESTS: HandleRfidDetectedEvent

        [Test]
        public void HandleRfidDetectedEvent_LadeskabStateAvailable_DeviceConnected_IdDetectedEventReceived_CorrectCalls()
        {
            // ARRANGE 
            // (Default: LadeskabState == Available)
            _mockChargeControl.IsConnected().Returns(true);

            // ACT
            int id = 123;
            _readerSubject.IdDetectedEvent += Raise.Event<EventHandler<int>>(this, id);

            // ASSERT
            _doorSubject.Received().LockDoor();
            _mockChargeControl.Received().StartCharge();
            _mockLogger.Received().LogDoorLocked(id);
            _mockDisplay.Received().ShowOccupied();
        }

        [Test]
        public void HandleRfidDetectedEvent_LadeskabStateAvailable_DeviceNotConnected_IdDetectedEventReceived_ErrorOnDisplayCalled()
        {
            // ARRANGE
            // (Default: LadeskabState == Available)
            _mockChargeControl.IsConnected().Returns(false);

            // ACT
            _readerSubject.IdDetectedEvent += Raise.Event<EventHandler<int>>(this, 123);

            // ASSERT
            _mockDisplay.Received().ShowConnectionError();
        }

        [Test]
        public void HandleRfidDetectedEvent_LadeskabStateDoorOpen_IdDetectedEventReceived_NothingHappened()
        {
            // ARRANGE
            _doorSubject.DoorStatusChanged +=
                Raise.EventWith(new DoorEventArgs { DoorStatus = DoorEventArgs.DoorState.Open }); // => LadeskabState = DoorOpen

            // ACT
            int id = 123;
            _readerSubject.IdDetectedEvent += Raise.Event<EventHandler<int>>(this, id);

            // ASSERT (Nothing happened)
            _mockDisplay.DidNotReceive().ShowOccupied();
            _mockDisplay.DidNotReceive().ShowConnectionError();
            _mockDisplay.DidNotReceive().ShowRemoveDevice();
            _mockDisplay.DidNotReceive().ShowWrongId();
        }

        [Test]
        public void HandleRfidDetectedEvent_LadeskabStateLocked_IdDetectedEventReceivedWithWrongId_ErrorOnDisplayCalled()
        {
            // ARRANGE
            _mockChargeControl.IsConnected().Returns(true);

            int oldId = 123;
            _readerSubject.IdDetectedEvent += Raise.Event<EventHandler<int>>(this, oldId); // => LadeskabState == locked, oldId = 123

            // ACT
            int id = 321;
            _readerSubject.IdDetectedEvent += Raise.Event<EventHandler<int>>(this, id);

            // ASSERT
            _mockDisplay.Received().ShowWrongId(); // oldId != id
        }

        [Test]
        public void HandleRfidDetectedEvent_LadeskabStateLocked_IdDetectedEventReceivedWithCorrectId_CorrectCalls()
        {
            // ARRANGE
            _mockChargeControl.IsConnected().Returns(true);

            int oldId = 123;
            _readerSubject.IdDetectedEvent += Raise.Event<EventHandler<int>>(this, oldId); // => LadeskabState == locked, oldId = 123

            // ACT
            int id = 123;
            _readerSubject.IdDetectedEvent += Raise.Event<EventHandler<int>>(this, id);

            // ASSERT (Correct Calls)
            _mockChargeControl.Received().StopCharge();
            _doorSubject.Received().UnlockDoor();
            _mockLogger.LogDoorUnlocked(id);
            _mockDisplay.Received().ShowRemoveDevice();
        }

        #endregion

        #region TESTS: HandleDoorStatusChangedEvent

        [Test]
        public void HandleDoorStatusChangedEvent_DoorOpenEventReceivedDeviceNotConnected_ShowInDisplayCalled()
        {
            _mockChargeControl.IsConnected().Returns(false);

            _doorSubject.DoorStatusChanged +=
                Raise.EventWith(new DoorEventArgs { DoorStatus = DoorEventArgs.DoorState.Open });

            _mockDisplay.Received().ShowConnectDevice();
        }


        [Test]
        public void HandleDoorStatusChangedEvent_DoorOpenEventReceivedDeviceConnected_ShowInDisplayNotCalled()
        {

            _mockChargeControl.IsConnected().Returns(true);

            _doorSubject.DoorStatusChanged +=
                Raise.EventWith(new DoorEventArgs { DoorStatus = DoorEventArgs.DoorState.Open });


            _mockDisplay.DidNotReceive().Show(Arg.Any<string>());
        }

        [Test]
        public void HandleDoorStatusChangedEvent_DoorClosedEventReceivedDeviceConnected_ShowInDisplayCalled()
        {
            _mockChargeControl.IsConnected().Returns(true);

            _doorSubject.DoorStatusChanged +=
                Raise.EventWith(new DoorEventArgs { DoorStatus = DoorEventArgs.DoorState.Closed });

            _mockDisplay.Received().ShowProvideId();
        }

        [Test]
        public void HandleDoorStatusChangedEvent_DoorClosedEventReceivedDeviceNotConnected_ShowInDisplayNotCalled()
        {
            _mockChargeControl.IsConnected().Returns(false);

            _doorSubject.DoorStatusChanged +=
                Raise.EventWith(new DoorEventArgs { DoorStatus = DoorEventArgs.DoorState.Closed });

            _mockDisplay.DidNotReceive().Show(Arg.Any<string>());
        }

        #endregion
    }

    [TestFixture]
    public class ChargeControlUnitTests
    {
        
        private ChargeControl _uut;
        private IUSBCharger _mockCharger;
        private IDisplay _mockDisplay;

        [SetUp]
        public void SetUp()
        {
            _mockCharger = Substitute.For<IUSBCharger>();
            _mockDisplay = Substitute.For<IDisplay>();

            _uut = new ChargeControl
            {
                Charger = _mockCharger,
                Display = _mockDisplay
            };
        }

        [Test]
        public void IsConnected_NoFunctionCalled_ReturnsSameValueAsProperty()
        {
            Assert.That(_uut.IsConnected(),Is.EqualTo(_mockCharger.Connected));
        }

        [Test]
        public void IsConnected_ChargerIsConnected_ReturnsTrue()
        {

            _mockCharger.Connected.Returns(true);
            Assert.That(_uut.IsConnected(), Is.True);
            
        }

        [Test]
        public void IsConnected_ChargerIsDisconnected_ReturnsFalse()
        {

            _mockCharger.Connected.Returns(false);
            Assert.That(_uut.IsConnected(), Is.False);

        }

        [Test]
        public void CurrentChanged_DisconnectedValue_NothingHappensCharging()
        {
            _mockCharger.CurrentValueEvent += Raise.EventWith(new CurrentEventArgs() { Current = 0 });
            _mockDisplay.DidNotReceive().ShowDeviceCharging();
        }
        [Test]
        public void CurrentChanged_DisconnectedValue_NothingHappensFullyCharged()
        {
            _mockCharger.CurrentValueEvent += Raise.EventWith(new CurrentEventArgs() { Current = 0 });
            _mockDisplay.DidNotReceive().ShowFullyCharged();
        }
        [Test]
        public void CurrentChanged_DisconnectedValue_NothingHappensOverload()
        {
            _mockCharger.CurrentValueEvent += Raise.EventWith(new CurrentEventArgs() { Current = 0 });
            _mockDisplay.DidNotReceive().ShowOverload();
        }

        [Test]
        public void CurrentChanged_ChargingValue_DisplayShowsCharging()
        {
            _mockCharger.CurrentValueEvent += Raise.EventWith(new CurrentEventArgs() {Current = 250});
            _mockDisplay.Received(1).ShowDeviceCharging();
        }

        [Test]
        public void CurrentChanged_ChargingValueTwoTimes_DisplayShowsChargingOnlyOnce()
        {
            _mockCharger.CurrentValueEvent += Raise.EventWith(new CurrentEventArgs() { Current = 250 });
            _mockCharger.CurrentValueEvent += Raise.EventWith(new CurrentEventArgs() { Current = 400 });
            _mockDisplay.Received(1).ShowDeviceCharging();
        }

        [Test]
        public void CurrentChanged_FullyChargedValue_DisplayShowsFullyCharged()
        {
            _mockCharger.CurrentValueEvent += Raise.EventWith(new CurrentEventArgs() { Current = 3 });
            _mockDisplay.Received(1).ShowFullyCharged();
        }

        [Test]
        public void CurrentChanged_FullyChargedValueTwoTimes_DisplayShowsFullyChargedOnlyOnce()
        {
            _mockCharger.CurrentValueEvent += Raise.EventWith(new CurrentEventArgs() { Current = 2 });
            _mockCharger.CurrentValueEvent += Raise.EventWith(new CurrentEventArgs() { Current = 4 });
            _mockDisplay.Received(1).ShowFullyCharged();
        }

        [Test]
        public void CurrentChanged_OverloadValue_DisplayShowsOverload()
        {
            _mockCharger.CurrentValueEvent += Raise.EventWith(new CurrentEventArgs() { Current = 750 });
            _mockDisplay.Received(1).ShowOverload();
        }

        [Test]
        public void CurrentChanged_OverloadValueTwoTimes_DisplayShowsOverloadOnlyOnce()
        {
            _mockCharger.CurrentValueEvent += Raise.EventWith(new CurrentEventArgs() { Current = 700 });
            _mockCharger.CurrentValueEvent += Raise.EventWith(new CurrentEventArgs() { Current = 750 });
            _mockDisplay.Received(1).ShowOverload();
        }


        [Test]
        public void StartCharge_ChargerReceivesStartCall()
        {
            _uut.StartCharge();
            _mockCharger.Received(1).StartCharge();
        }

        [Test]
        public void StartCharge_ChargerReceivesNoStopCall()
        {
            _uut.StartCharge();
            _mockCharger.DidNotReceive().StopCharge();
        }

        [Test]
        public void StopCharge_ChargerReceivesStopCall()
        {
            _uut.StopCharge();
            _mockCharger.Received(1).StopCharge();
        }

        [Test]
        public void StopCharge_ChargerReceivesNoStartCall()
        {
            _uut.StopCharge();
            _mockCharger.DidNotReceive().StartCharge();
        }

    }

    [TestFixture]
    public class USBChargerUnitTests
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

    [TestFixture]
    public class DisplayUnitTests
    {
        private Display _uut;
        private StringWriter _output;

        [SetUp]
        public void Setup()
        {
            _uut = new Display();

            _output = new StringWriter();
            Console.SetOut(_output);
        }

        [TestCase("")]
        [TestCase("123")]
        [TestCase("string")]
        public void Show_CalledWithMessage_MessageIsWrittenCorrectlyOnScreen(string msg)
        {
            _uut.Show(msg);

            Assert.That(_output.ToString(), Is.EqualTo("On Display: '" + msg + "'\r\n"));
        }

        [Test]
        public void ShowOverload_Called_MessageIsWrittenCorrectlyOnScreen()
        {
            _uut.ShowOverload();

            Assert.That(_output.ToString(), Is.EqualTo("On Display: '" + "Error during charging - Remove device immediately" + "'\r\n"));
        }

        [Test]
        public void ShowConnectDevice_Called_MessageIsWrittenCorrectlyOnScreen()
        {
            _uut.ShowConnectDevice();

            Assert.That(_output.ToString(), Is.EqualTo("On Display: '" + "Please connect device." + "'\r\n"));
        }

        [Test]
        public void ShowConnectionError_Called_MessageIsWrittenCorrectlyOnScreen()
        {
            _uut.ShowConnectionError();

            Assert.That(_output.ToString(), Is.EqualTo("On Display: '" + "Connection Error: Your device is not properly connected. Try again." + "'\r\n"));
        }

        [Test]
        public void ShowFullyCharged_Called_MessageIsWrittenCorrectlyOnScreen()
        {
            _uut.ShowFullyCharged();

            Assert.That(_output.ToString(), Is.EqualTo("On Display: '" + "Phone Fully Charged" + "'\r\n"));
        }

        [Test]
        public void ShowDeviceCharging_Called_MessageIsWrittenCorrectlyOnScreen()
        {
            _uut.ShowDeviceCharging();

            Assert.That(_output.ToString(), Is.EqualTo("On Display: '" + "Phone Charging" + "'\r\n"));
        }

        [Test]
        public void ShowOccupied_Called_MessageIsWrittenCorrectlyOnScreen()
        {
            _uut.ShowOccupied();

            Assert.That(_output.ToString(), Is.EqualTo("On Display: '" + "Occupied: Door is locked and your device is charging.Use your RFID tag to unlock door." + "'\r\n"));
        }

        [Test]
        public void ShowProvideId_Called_MessageIsWrittenCorrectlyOnScreen()
        {
            _uut.ShowProvideId();

            Assert.That(_output.ToString(), Is.EqualTo("On Display: '" + "Provide RFID to lock." + "'\r\n"));
        }

        [Test]
        public void ShowRemoveDevice_Called_MessageIsWrittenCorrectlyOnScreen()
        {
            _uut.ShowRemoveDevice();

            Assert.That(_output.ToString(), Is.EqualTo("On Display: '" + "Remove device." + "'\r\n"));
        }

        [Test]
        public void ShowWrongId_Called_MessageIsWrittenCorrectlyOnScreen()
        {
            _uut.ShowWrongId();

            Assert.That(_output.ToString(), Is.EqualTo("On Display: '" + "Wrong RFID tag. Use your RFID tag to try again." + "'\r\n"));
        }


    }


    [TestFixture]
    public class LogFileUnitTests
    {
        private LogFile _uut;
        private StringWriter _output;

        [SetUp]
        public void Setup()
        {
            _uut = new LogFile("testFile.txt");
            _uut.CreateFile(); //Denne setup sikrer at filen kun eksistere i testen
        }

        [TearDown]
        public void TearDown()
        {
            _uut.DeleteFile(); //Denne teardown sikrer at filen kun eksisterer i testen
        }

        [Test]
        public void FileExists_CreateFileCalled_ReturnsTrue()
        {
            Assert.That(_uut.FileExist(),Is.True);
        }

        [Test]
        public void FileExists_DeleteFileCalled_ReturnsFalse()
        {
            _uut.DeleteFile();
            Assert.That(_uut.FileExist(), Is.False);
        }

        [Test]
        public void CreateFile_ReadLastMessageFromFile_ReturnsExpectedMessage()
        {
            var lastMessage = File.ReadLines(_uut.path).Last();
            //Assert
            Assert.That(lastMessage,Is.EqualTo(_uut.LastMessage));
        }

        [Test]
        public void AppendText_ReadLastMessageFromFile_ReturnsExpectedMessage()
        {
            //Act
            var text = "SomeText";
            _uut.AppendTextToFile(text);
            var lastMessage = File.ReadLines(_uut.path).Last();

            //Assert
            Assert.That(lastMessage, Is.EqualTo(text));
        }

        [Test]
        public void LogDoorLocked_RunWhenNoFileExists_CreatesFile()
        {
            _uut.DeleteFile();

            _uut.LogDoorLocked(1);

            Assert.That(_uut.FileExist, Is.True);
        }

        [Test]
        public void LogDoorLocked_RunWhenNoFileExists_AppendsText()
        {
            _uut.DeleteFile();

            _uut.LogDoorLocked(1);
            var lastMessage = File.ReadLines(_uut.path).Last();

            Assert.That(lastMessage, Is.EqualTo(_uut.LastMessage));
        }

        [Test]
        public void LogDoorLocked_RunWhenFileExists_AppendsText()
        {
            _uut.LogDoorLocked(1);
            var lastMessage = File.ReadLines(_uut.path).Last();

            Assert.That(lastMessage, Is.EqualTo(_uut.LastMessage));
        }



        [Test]
        public void LogDoorUnlocked_RunWhenNoFileExists_CreatesFile()
        {
            _uut.DeleteFile();

            _uut.LogDoorUnlocked(1);

            Assert.That(_uut.FileExist, Is.True);
        }

        [Test]
        public void LogDoorUnlocked_RunWhenNoFileExists_AppendsText()
        {
            _uut.DeleteFile();

            _uut.LogDoorUnlocked(1);
            var lastMessage = File.ReadLines(_uut.path).Last();

            Assert.That(lastMessage, Is.EqualTo(_uut.LastMessage));
        }

        [Test]
        public void LogDoorUnlocked_RunWhenFileExists_AppendsText()
        {
            _uut.LogDoorUnlocked(1);
            var lastMessage = File.ReadLines(_uut.path).Last();

            Assert.That(lastMessage, Is.EqualTo(_uut.LastMessage));
        }


    }





}
