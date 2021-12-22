using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using Ddd.Infrastructure;

namespace Ddd.Taxi.Domain
{
    // In real aplication it whould be the place where database is used to find driver by its Id.
    // But in this exercise it is just a mock to simulate database
    public class DriversRepository
    {
        List<Driver> dataBase = new List<Driver>
        {
            new Driver(15, new PersonName("Drive", "Driverson"), new Car("Lada sedan", "Baklazhan", "A123BT 66"))
        };

        public void Write(Driver driver)
        {
            dataBase.Add(driver);
        }

        public Driver Read(int driverId)
        {
            if (!dataBase.Any(d => d.Id == driverId))
                throw new Exception("Unknown driver id " + driverId);
            return dataBase.Find(driver => driverId == driver.Id);
        }
    }

    public class TaxiApi : ITaxiApi<TaxiOrder>
    {
        private readonly DriversRepository driversRepo;
        private readonly Func<DateTime> currentTime;
        private int idCounter;

        public TaxiApi(DriversRepository driversRepo, Func<DateTime> currentTime)
        {
            this.driversRepo = driversRepo;
            this.currentTime = currentTime;
        }

        public TaxiOrder CreateOrderWithoutDestination(string firstName, string lastName, 
            string street, string building)
        {
            return new TaxiOrder
                (
                    idCounter++,
                    new PersonName(firstName, lastName),
                    new Address(street, building),
                    currentTime()
                );
        }

        public void UpdateDestination(TaxiOrder order, string street, string building)
        {
            order.UpdateDestination(new Address(street, building));
        }

        public void AssignDriver(TaxiOrder order, int driverId)
        {
            order.AssignDriver(driverId, driversRepo, currentTime());
        }

        public void UnassignDriver(TaxiOrder order)
        {
            order.UnassignDriver();
        }

        public string GetDriverFullInfo(TaxiOrder order)
        {
            return order.GetDriverFullInfo();
        }

        public string GetShortOrderInfo(TaxiOrder order)
        {
            return order.GetShortOrderInfo();
        }

        public void Cancel(TaxiOrder order)
        {
            order.Cancel(currentTime());
        }

        public void StartRide(TaxiOrder order)
        {
            order.StartRide(currentTime());
        }

        public void FinishRide(TaxiOrder order)
        {
            order.FinishRide(currentTime());
        }
    }

    public class TaxiOrder : Entity<int>
    {
        public PersonName ClientName { get; private set; }
        public Address Start { get; private set; }
        public Address Destination { get; private set; }
        public Driver Driver { get; private set; }
        public Car Car { get => Driver.Car; }
        public TaxiOrderStatus Status { get; private set; }
        public TimeInfo TimeInfo { get; private set; }

        public TaxiOrder(int id, PersonName clientName, Address startAddress, Address destinationAddress,
            Driver driver, TaxiOrderStatus status, TimeInfo timeInfo) : base(id)
        {
            ClientName = clientName;
            Start = startAddress;
            Destination = destinationAddress;
            Driver = driver;
            Status = status;
            TimeInfo = timeInfo;
        }

        public TaxiOrder(int id, PersonName clientName, Address startAddress, DateTime time)
            : this(id, clientName, startAddress, new Address(null, null),
                new Driver(0, new PersonName(null, null), new Car() { } ),
                TaxiOrderStatus.WaitingForDriver, new TimeInfo { CreationTime = time })
        { }

        public void UpdateDestination(Address adress)
        {
            Destination = adress;
        }

        public void AssignDriver(int driverId, DriversRepository driversRepo, DateTime assignmentTime)
        {
            if (Status != TaxiOrderStatus.WaitingForDriver)
                throw new InvalidOperationException();
            var driver = driversRepo.Read(driverId);
            if (driver != null)
            {
                Driver = driver;
                Status = TaxiOrderStatus.WaitingCarArrival;
                TimeInfo.DriverAssignmentTime = assignmentTime;
            }
        }

        public void UnassignDriver()
        {
            if (Driver.FullName.FirstName == null || Driver.Car.CarModel == null)
                throw new InvalidOperationException($"Can't unassign while {Status}");
            if (Status == TaxiOrderStatus.WaitingForDriver 
                || Status == TaxiOrderStatus.InProgress)
                throw new InvalidOperationException($"Can't unassign while {Status}");
            //Driver.Id = 0;
            Driver = new Driver(0, new PersonName(null, null), new Car());
            Status = TaxiOrderStatus.WaitingForDriver;
        }

        public string GetDriverFullInfo()
        {
            if (Status == TaxiOrderStatus.WaitingForDriver) return null;
            return string.Join(" ",
                "Id: " + Driver.Id,
                "DriverName: " + FormatData(Driver.FullName.FirstName, Driver.FullName.LastName),
                "Color: " + Car.CarColor,
                "CarModel: " + Car.CarModel,
                "PlateNumber: " + Car.CarPlateNumber);
        }

        public string GetShortOrderInfo()
        {
            return string.Join(" ",
                "OrderId: " + Id,
                "Status: " + Status,
                "Client: " + FormatData(ClientName.FirstName, ClientName.LastName),
                "Driver: " + FormatData(Driver.FullName.FirstName, Driver.FullName.LastName),
                "From: " + FormatData(Start.Street, Start.Building),
                "To: " + FormatData(Destination.Street, Destination.Building),
                "LastProgressTime: " + GetLastProgressTime()
                    .ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        }

        private string FormatData(string firstName, string lastName)
        {
            return string.Join(" ", new[] { firstName, lastName }.Where(n => n != null));
        }

        private DateTime GetLastProgressTime()
        {
            if (Status == TaxiOrderStatus.WaitingForDriver) return TimeInfo.CreationTime;
            if (Status == TaxiOrderStatus.WaitingCarArrival) return TimeInfo.DriverAssignmentTime;
            if (Status == TaxiOrderStatus.InProgress) return TimeInfo.StartRideTime;
            if (Status == TaxiOrderStatus.Finished) return TimeInfo.FinishRideTime;
            if (Status == TaxiOrderStatus.Canceled) return TimeInfo.CancelTime;
            throw new NotSupportedException(Status.ToString());
        }

        public void Cancel(DateTime cancelTime)
        {
            if (Status == TaxiOrderStatus.InProgress)
                throw new InvalidOperationException();
            Status = TaxiOrderStatus.Canceled;
            TimeInfo.CancelTime = cancelTime;
        }

        public void StartRide(DateTime startTime)
        {
            if (Driver.FullName.FirstName == null || Driver.Car.CarModel == null)
                throw new InvalidOperationException();
            Status = TaxiOrderStatus.InProgress;
            TimeInfo.StartRideTime = startTime;
        }

        public void FinishRide(DateTime finishTime)
        {
            if (Driver.FullName.FirstName == null || Driver.Car.CarModel == null)
                throw new InvalidOperationException();
            if (Status != TaxiOrderStatus.InProgress)
                throw new InvalidOperationException();
            Status = TaxiOrderStatus.Finished;
            TimeInfo.FinishRideTime = finishTime;
        }
    }

    public class Client
    {
        public PersonName FullName;
    }

    public class Driver : Entity<int>
    {
        public PersonName FullName;
        public Car Car { get; }

        public Driver(int id, PersonName personName, Car car): base(id) 
        {
            FullName = personName;
            Car = car;
        }
    }

    public class Car : ValueType<Car>
    {
        public string CarColor;
        public string CarModel;
        public string CarPlateNumber;

        public Car() { }

        public Car(string model, string color,  string plateNumber)
        {
            CarModel = model;
            CarColor = color;
            CarPlateNumber = plateNumber;
        }
    }

    public class TimeInfo : ValueType<TimeInfo>
    {
        public DateTime CreationTime;
        public DateTime DriverAssignmentTime;
        public DateTime CancelTime;
        public DateTime StartRideTime;
        public DateTime FinishRideTime;
    }
}
