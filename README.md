# Digital Twin with IOT Devices using ROS-Unity Integration

This project implements a digital twin system that connects an **Arduino Uno** with an **MPU9250 IMU sensor** to **ROS2 Jazzy** via WSL on Windows, and publishes/receives data to/from **Unity** using the ROS–Unity bridge. The system enables real-world physical movements to be mirrored in a virtual 3D environment in real-time.

## Overview

![System Architecture](https://via.placeholder.com/800x400?text=Digital+Twin+System+Architecture)

### Key Components
- **Hardware Layer**: Arduino Uno board with MPU9250 IMU sensor
- **Middleware Layer**: ROS2 Jazzy running in WSL2 on Windows
- **Visualization Layer**: Unity 3D environment
- **Communication**: Serial connection (Arduino→WSL) and TCP/IP (WSL→Unity)

## 🟩 Arduino Setup

### ✅ Prerequisites
- **Arduino IDE** installed from [arduino.cc/en/software](https://www.arduino.cc/en/software)
- Arduino Uno board
- MPU9250 IMU sensor
- USB cable for connecting Arduino to computer

### 🧰 Wiring Connections
Connect the MPU9250 IMU sensor to the Arduino Uno following these connections:
- VCC → 3.3V
- GND → GND
- SCL → A5 (Arduino analog pin 5)
- SDA → A4 (Arduino analog pin 4)

For detailed pinout information, refer to: [IMU Breakout - MPU-9250 Wiki](http://wiki.sunfounder.cc/index.php?title=IMU_Breakout_-_MPU-9250)

### ⬆️ Upload Process
1. Open the `MovementOfRobot.ino` sketch in Arduino IDE
2. Select "Arduino Uno" from the Board menu
3. Select the correct COM port from the Port menu
4. Click the Upload button
5. Verify successful upload in the console output

### 📊 Data Format
The Arduino code captures and transmits the following data:
- Accelerometer readings (X, Y, Z axes)
- Gyroscope readings (X, Y, Z axes)
- Magnetometer readings (X, Y, Z axes)

Data is transmitted via serial at 115200 baud rate in JSON format:
```
{"accel":{"x":0.05,"y":-0.02,"z":1.03},"gyro":{"x":0.01,"y":0.01,"z":0.00},"mag":{"x":23.5,"y":-16.2,"z":42.1}}
```

## 🟦 ROS2 Setup (WSL2 on Windows)

### ✅ Prerequisites
- Windows 10/11 with WSL2 enabled
- Ubuntu 22.04 LTS installed in WSL2
- ROS2 Jazzy installed in WSL2
- usbipd-win installed on Windows host

### 🌐 USB Device Passthrough to WSL2
In **PowerShell with Administrator privileges**:
```
usbipd list
usbipd attach --wsl --busid <your-busid>  # Example: 1-3
```

In **WSL Terminal**, verify the connection:
```
lsusb
ls -l /dev/tty*
sudo minicom -D /dev/ttyACM0 -b 115200  # To test serial communication
```

### 🛠️ ROS2 Workspace Setup
The workspace is organized as follows:
```
~/ros2_ws/
├── src/
│   ├── arduino_interface/      # Package for Arduino communication
│   └── ros_tcp_endpoint/       # Package for Unity communication
├── build/                      # Build files (generated)
├── install/                    # Install files (generated)
└── log/                        # Log files (generated)
```

### 🚀 Running the ROS2 Node
Execute these commands to start the Arduino interface node:
```
cd ~/ros2_ws
source install/setup.bash
sudo chmod 666 /dev/ttyACM0    # Set appropriate permissions
ros2 run arduino_interface arduino_node
```

### 📡 Verifying ROS Topics
In a separate terminal, check that topics are being published:
```
source ~/ros2_ws/install/setup.bash
ros2 topic list                # Should show /arduino_data and /pos_rot
ros2 topic echo /arduino_data  # Should display the JSON data from Arduino
ros2 topic echo /pos_rot       # Should show the processed position/rotation data
```

## 🎮 Unity Integration

### ✅ Prerequisites
- Unity 2022.3 LTS or newer
- ROS-TCP-Connector package installed in Unity

### 🌐 Starting the ROS-Unity Bridge
First, find your WSL2 IP address:
```
hostname -I
```

Then start the ROS TCP endpoint server:
```
source ~/ros2_ws/install/setup.bash
ros2 run ros_tcp_endpoint default_server_endpoint --ros-args -p ROS_IP:=<your-WSL-IP>
```

Example:
```
ros2 run ros_tcp_endpoint default_server_endpoint --ros-args -p ROS_IP:=172.29.236.96
```

### 🎲 Unity Project Configuration
1. Open the Unity project
2. Configure the ROS connection in Unity:
   - Navigate to Robotics → ROS Settings
   - Set the ROS IP Address to your WSL IP
   - Set the ROS Port to 10000
3. Ensure the `DigitalTwinController` script is attached to your 3D model
4. Play the scene to start the digital twin

### 🔍 Testing Unity Connection
To verify the Unity connection is working:
1. Run both the Arduino node and ROS-TCP endpoint in WSL
2. Play the Unity scene
3. Move the physical IMU sensor
4. Observe the corresponding movement in the Unity scene

## 🔁 Full Operation Workflow

1. **Start Hardware**:
   - Connect Arduino to computer
   - Ensure IMU sensor is properly connected

2. **Connect Arduino to WSL**:
   ```
   usbipd list
   usbipd attach --wsl --busid <your-busid>
   ```

3. **Start ROS Node**:
   ```
   cd ~/ros2_ws
   source install/setup.bash
   sudo chmod 666 /dev/ttyACM0
   ros2 run arduino_interface arduino_node
   ```

4. **Start ROS-Unity Bridge**:
   ```
   source ~/ros2_ws/install/setup.bash
   ros2 run ros_tcp_endpoint default_server_endpoint --ros-args -p ROS_IP:=<your-WSL-IP>
   ```

5. **Start Unity**:
   - Open Unity project
   - Play the scene
   - Verify data flow in Unity Console

## ⚠️ Troubleshooting

### Arduino Connection Issues
- **Problem**: Cannot detect Arduino in WSL
  - **Solution**: Verify USB passthrough with `lsusb`, try detaching/reattaching with usbipd

- **Problem**: Permission denied when accessing /dev/ttyACM0
  - **Solution**: Run `sudo chmod 666 /dev/ttyACM0`

### ROS Communication Issues
- **Problem**: Topics not showing up in `ros2 topic list`
  - **Solution**: Check if node is running with `ros2 node list`
  - **Solution**: Verify Arduino is properly connected and sending data

- **Problem**: Data not being published to topics
  - **Solution**: Check Arduino serial output with `minicom` or Arduino IDE Serial Monitor
  - **Solution**: Restart the ROS node

### Unity Connection Issues
- **Problem**: Unity not receiving data
  - **Solution**: Verify ROS IP address is correct
  - **Solution**: Check Windows Firewall settings for TCP port 10000
  - **Solution**: Restart ROS-Unity bridge

## 📊 Performance Considerations

### Data Rate
- Arduino sends data at approximately 100Hz
- ROS processes data at approximately 50Hz
- Unity frame rate varies by hardware (typically 60-90Hz)

### System Latency
- End-to-end latency is approximately 50-100ms
- Major contributors to latency:
  - Serial communication (~10ms)
  - WSL networking overhead (~15ms)
  - Unity rendering pipeline (~20ms)

### Optimization Tips
- Reduce Arduino sampling rate if CPU usage is high
- Adjust ROS buffer sizes for smoother data flow
- Use Unity's fixed update for consistent physics

## 🚀 Future Enhancements

### Additional Features
- Multiple sensor support for complex articulated systems
- Bidirectional control (Unity → Arduino)
- Data recording and playback functionality
- Machine learning for predictive digital twin behavior

### Integration Possibilities
- Extended Reality (XR) visualization
- Cloud-based data storage and analysis
- Multi-user collaborative interaction
- IoT platform integration

## 📚 Resources

### Documentation
- [ROS2 Documentation](https://docs.ros.org/en/jazzy)
- [Unity-ROS Integration](https://github.com/Unity-Technologies/ROS-TCP-Connector)
- [Arduino MPU9250 Library](https://github.com/hideakitai/MPU9250)

### Community Support
- [ROS Answers](https://answers.ros.org)
- [Unity Forum](https://forum.unity.com)
- [Arduino Forum](https://forum.arduino.cc)

## 👨‍💻 Project Information

- **Intern:** [Your Name]
- **Mentor:** Praveen Krishna
- **Project Duration:** [Start Date] - [End Date]
- **Contact:** [Your Email]

---

## Quick Reference Commands

### Windows PowerShell
```
usbipd list
usbipd attach --wsl --busid <busid>
usbipd detach --busid <busid>
```

### WSL Terminal
```
hostname -I                             # Get WSL IP address
ls -l /dev/tty*                         # List serial devices
sudo chmod 666 /dev/ttyACM0             # Set device permissions
source ~/ros2_ws/install/setup.bash     # Source ROS workspace
ros2 run arduino_interface arduino_node # Run Arduino node
ros2 topic list                         # List active topics
ros2 topic echo /arduino_data           # View raw sensor data
```
