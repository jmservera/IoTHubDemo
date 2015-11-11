#include "pch.h"
#include "DHTReaderClass.h"
#include "SensorData.h"

using namespace DHTReader;
using namespace Platform;
using namespace Windows::Devices::Gpio;


void DHTReader::Dht11::Init(GpioPin^ Pin)
{
	// Use InputPullUp if supported, otherwise fall back to Input (floating)
	this->inputDriveMode =
		Pin->IsDriveModeSupported(GpioPinDriveMode::InputPullUp) ?
		GpioPinDriveMode::InputPullUp : GpioPinDriveMode::Input;

	Pin->SetDriveMode(this->inputDriveMode);
	this->pin = Pin;
}

_Use_decl_annotations_
HRESULT DHTReader::Dht11::Sample(DHTReader::Dht11Reading& Reading)
{
	Reading = Dht11Reading();

	LARGE_INTEGER qpf;
	QueryPerformanceFrequency(&qpf);

	// This is the threshold used to determine whether a bit is a '0' or a '1'.
	// A '0' has a pulse time of 76 microseconds, while a '1' has a
	// pulse time of 120 microseconds. 110 is chosen as a reasonable threshold.
	// We convert the value to QPF units for later use.
	const unsigned int oneThreshold = static_cast<unsigned int>(
		110LL * qpf.QuadPart / 1000000LL);

	// Latch low value onto pin
	this->pin->Write(GpioPinValue::Low);

	// Set pin as output
	this->pin->SetDriveMode(GpioPinDriveMode::Output);

	// Wait for at least 18 ms
	Sleep(SAMPLE_HOLD_LOW_MILLIS);

	// Set pin back to input
	this->pin->SetDriveMode(this->inputDriveMode);

	GpioPinValue previousValue = this->pin->Read();

	// catch the first rising edge
	const ULONG initialRisingEdgeTimeoutMillis = 1;
	ULONGLONG endTickCount = GetTickCount64() + initialRisingEdgeTimeoutMillis;
	for (;;) {
		if (GetTickCount64() > endTickCount) {
			return HRESULT_FROM_WIN32(ERROR_TIMEOUT);
		}

		GpioPinValue value = this->pin->Read();
		if (value != previousValue) {
			// rising edgue?
			if (value == GpioPinValue::High) {
				break;
			}
			previousValue = value;
		}
	}

	LARGE_INTEGER prevTime = { 0 };

	const ULONG sampleTimeoutMillis = 10;
	endTickCount = GetTickCount64() + sampleTimeoutMillis;

	// capture every falling edge until all bits are received or
	// timeout occurs
	for (unsigned int i = 0; i < (Reading.bits.size() + 1);) {
		if (GetTickCount64() > endTickCount) {
			return HRESULT_FROM_WIN32(ERROR_TIMEOUT);
		}

		GpioPinValue value = this->pin->Read();
		if ((previousValue == GpioPinValue::High) && (value == GpioPinValue::Low)) {
			// A falling edge was detected
			LARGE_INTEGER now;
			QueryPerformanceCounter(&now);

			if (i != 0) {
				unsigned int difference = static_cast<unsigned int>(
					now.QuadPart - prevTime.QuadPart);
				Reading.bits[Reading.bits.size() - i] =
					difference > oneThreshold;
			}

			prevTime = now;
			++i;
		}

		previousValue = value;
	}

	if (!Reading.IsValid()) {
		// checksum mismatch
		return HRESULT_FROM_WIN32(ERROR_INVALID_DATA);
	}

	return S_OK;
}

DHTReader::DHTReaderClass::DHTReaderClass(int pinNumber)
{
	GpioController^ controller = GpioController::GetDefault();
	if (!controller) {
		this->StatusText = L"GPIO is not available on this system";
		return;
	}

	GpioPin^ pin;
	try {
		pin = controller->OpenPin(pinNumber);
	}
	catch (Exception^ ex) {
		this->StatusText= L"Failed to open GPIO pin: " + ex->Message;
		return;
	}

	this->dht11.Init(pin);

	this->StatusText = L"Status: Initialized Successfully";

}

DHTReader::SensorData DHTReader::DHTReaderClass::Read()
{
	HRESULT sensorHr;
	Dht11Reading reading;
	SensorData d;

	int retryCount = 0;
	do {
		sensorHr = this->dht11.Sample(reading);
	} while (FAILED(sensorHr) && (++retryCount < 50));

	if (FAILED(sensorHr)) {
		this->failures++;

		switch (sensorHr) {
		case __HRESULT_FROM_WIN32(ERROR_IO_DEVICE):
			this->StatusText = L"Did not catch all falling edges";
			break;
		case __HRESULT_FROM_WIN32(ERROR_TIMEOUT):
			this->StatusText = L"Timed out waiting for sample";
			break;
		case __HRESULT_FROM_WIN32(ERROR_INVALID_DATA):
			this->StatusText = L"Checksum validation failed";
			break;
		default:
			this->StatusText = L"Failed to get reading";
		}

		return d;
	}

	HRESULT hr;
	wchar_t buf[128];

	hr = StringCchPrintfW(
		buf,
		ARRAYSIZE(buf),
		L"Succeeded (%d %s)",
		retryCount,
		(retryCount == 1) ? L"retry" : L"retries");
	if (FAILED(hr)) {
		throw ref new Exception(hr, L"Failed to print string");
	}
	OutputDebugString(buf);

	this->StatusText = ref new String(buf);

	hr = StringCchPrintfW(
		buf,
		ARRAYSIZE(buf),
		L"Failures (%d)",
		this->failures);
	if (FAILED(hr)) {
		throw ref new Exception(hr, L"Failed to print string");
	}
	this->failures = 0;
	OutputDebugString(buf);

	d.Humidity = reading.Humidity();
	d.Temp = reading.Temperature();
	d.IsValid = reading.IsValid();
	return d;
}
