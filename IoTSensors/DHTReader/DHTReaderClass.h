#pragma once

#include<SensorData.h>

namespace DHTReader
{
	struct Dht11Reading {

		bool IsValid() const
		{
			unsigned long long value = this->bits.to_ullong();
			unsigned int checksum =
				((value >> 32) & 0xff) +
				((value >> 24) & 0xff) +
				((value >> 16) & 0xff) +
				((value >> 8) & 0xff);

			return (checksum & 0xff) == (value & 0xff);
		}

		double Humidity() const
		{
			unsigned long long value = this->bits.to_ullong();
			return (((value >> 24) & 0xff00) + ((value >> 24) & 0xff)) / 10.0;
		}

		double Temperature() const
		{
			unsigned long long value = this->bits.to_ullong();
			double f = (((value >> 8) & 0x7f00) + ((value >> 8) & 0xff)) / 10.0;
			if ((value >> 16) & 0x80) f *= -1;
			return f;
		}

		std::bitset<40> bits;
	};

	class Dht11
	{
		enum { SAMPLE_HOLD_LOW_MILLIS = 18 };

	public:

		Dht11() :
			pin(nullptr),
			inputDriveMode(Windows::Devices::Gpio::GpioPinDriveMode::Input)
		{ }

		void Init(Windows::Devices::Gpio::GpioPin^ Pin);

		HRESULT Sample(_Out_ Dht11Reading& Reading);

		bool PullResistorRequired() const
		{
			return inputDriveMode != Windows::Devices::Gpio::GpioPinDriveMode::InputPullUp;
		}

	private:
		Windows::Devices::Gpio::GpioPin^ pin;
		Windows::Devices::Gpio::GpioPinDriveMode inputDriveMode;
	};

	public ref class DHTReaderClass sealed
	{
	public:
		DHTReaderClass(int pinNumber);
		
		property Platform::String^ Status {
			Platform::String^ get() { return StatusText; }
		}
		
		DHTReader::SensorData Read();

	private:
		
		Platform::String^ StatusText;

		Dht11 dht11;
		int failures = 0;
	};
}
