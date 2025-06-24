# SAPAPP
The Swiss-Army Programmer (SAP) Application streamlines firmware uploading by integrating complex configurations into an automated process. Developed using C# and Windows Presentation Foundation (WPF), it ensures compatibility across multiple Printed Circuit Boards (PCBs).

## Documentation
Documentation for current release is on Github Pages.

## Getting the SAP APP
Installation guide coming soon.

## User Guide
### Step 1: Launching the Application
Open the **Swiss-Army Programmer** from the desktop or start menu.

### Step 2: Selecting the Product & Part Board
1. Locate the **Product dropdown menu** on the interface.
2. Select the desired **product** from the available options.
3. Choose the correct title for the **Printed Circuit Board (PCB)** firmware you would like to flash using the second dropdown.
4. Confirm selection before proceeding.

### Step 3: USB Microcontroller Interfacing
**Connect the Circuit Board to the Computer**:
  - **MSP430 Microcontrollers** → Use **MSP-FET Programming Tool**.
  - **Microchip ATMega** → Use **Atmel AVR ISP Mk2 Programming Tool**.
  - **STM32 Microcontrollers** → Use **USB Cable of choice**.

### Step 4: Uploading Firmware
1. Ensure the correct **product and PCB firmware** have been selected.
2. Click the **Start button** to initiate the firmware upload process.
3. Monitor the **progress bar** for real-time feedback and status messages.
4. If necessary, click **Stop** to halt the upload.

## Requirements
### Supported Microcontrollers
- **Texas Instruments** MSP430
- **Microchip** ATmega
- **STMicroelectronics** STM32
- **Texas Instruments** Battery Fuel Gauges

### Required Hardware & Debugging Tools:
- MSP-FET for MSP430 microcontrollers
- Microchip avrispmk2 for Microchip ATmega microcontrollers
- Arduino UNO R3 for Ti Fuel Gauges

### Required Software:
For flashing certain microcontrollers, **SAP APP cannot function on its own**
  - [**STM32 Cube Programmer**](https://www.st.com/en/development-tools/stm32cubeprog.html#st-get-software) for STM32 Microcontrollers
  - [**AVRDUDE**](https://github.com/avrdudes/avrdude) for ATMega Microcontrollers
  - [**TI Uniflash**](https://www.ti.com/tool/UNIFLASH?utm_source=google&utm_medium=cpc&utm_campaign=epd-der-null-44700045336317962_prodfolderdynamic-cpc-pf-google-ww_en_int&utm_content=prodfolddynamic&ds_k=DYNAMIC+SEARCH+ADS&DCM=yes&gad_source=1&gad_campaignid=12788797621&gbraid=0AAAAAC068F3MzEJzVnVkTWaFJKTRlscxS&gclid=CjwKCAjwmenCBhA4EiwAtVjzmnQSBu5Cp1eznJ5YPZoRNaYYrawTZW6Mz7yKIpYkqecOK8olu9FbHhoC0_AQAvD_BwE&gclsrc=aw.ds#downloads) for MSP430 Microcontrollers

## Resources

For full documentation, visit the [Official Swiss-Army Programmer Wiki](https://github.com/SensitTechnologies/SAPAPP/wiki).
---


