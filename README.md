# Arduino to ROS2 to Unity Integration

This project connects an **Arduino Uno** (with an **MPU9250 IMU sensor**) to **ROS2 Jazzy** via WSL on Windows, and publishes/receives data to/from **Unity** using the ROS–Unity bridge.

---

## 🟩 Arduino Setup

### ✅ Prerequisites
- Install the **Arduino IDE** from [https://www.arduino.cc/en/software](https://www.arduino.cc/en/software).
- Tested on **Arduino Uno** with **MPU9250 IMU sensor**.

### 🧰 Wiring Connections
Ensure the IMU is properly connected to the Arduino according to your sensor's specifications.

### ⬆️ Upload Code
1. Open the provided Arduino sketch.
2. Select the correct board and port in the IDE.
3. Click **Upload**.

---

## 🟦 ROS2 (WSL2 on Windows)

### ✅ Prerequisites
- Install **ROS2 Jazzy** in WSL2 (Ubuntu) following the [official guide](https://docs.ros.org/en/jazzy/Installation.html).

### ⚙️ Setup Serial Communication
In **PowerShell (Administrator)**:
```bash
usbipd list
usbipd attach --wsl --busid <your-busid>  # Example: 1-3
```

In **WSL Terminal**:
```bash
lsusb
ls -l /dev/tty*
sudo minicom -D /dev/ttyACM0 -b 115200
```

### 🛠️ Build and Run the Node
```bash
cd ~/ros2_ws/src
# Ensure your arduino_interface package is here
cd ~/ros2_ws
colcon build --packages-select arduino_interface
source install/setup.bash
sudo chmod 666 /dev/ttyACM0
ros2 run arduino_interface arduino_node
```

### 📡 Check ROS Topics
```bash
ros2 topic list
ros2 topic info /arduino_data
ros2 interface show std_msgs/msg/String
ros2 topic echo /arduino_data
```

---

## 🎮 Unity Integration

### 🌐 Unity Publisher (ROS to Unity)
Find your WSL IP:
```bash
hostname -I
```

Then run the ROS–Unity TCP server:
```bash
ros2 run ros_tcp_endpoint default_server_endpoint --ros-args -p ROS_IP:=<your-WSL-IP>
```

Example:
```bash
ros2 run ros_tcp_endpoint default_server_endpoint --ros-args -p ROS_IP:=172.29.236.96
```

Check Unity topic:
```bash
ros2 topic echo pos_rot
```

### 🖥️ Unity Subscriber (Unity to ROS)
Run Unity's ROS publishing demo:
```bash
ros2 run unity_robotics_demo color_publisher
```

### 🔁 Full Restart Commands (Quick Reuse)
```bash
usbipd list
usbipd attach --wsl --busid <your-busid>
cd ~/ros2_ws/src
source ~/ros2_ws/install/setup.bash
sudo chmod 666 /dev/ttyACM0
ros2 run arduino_interface arduino_node
source ~/ros2_ws/install/setup.bash
ros2 run ros_tcp_endpoint default_server_endpoint --ros-args -p ROS_IP:=<your-WSL-IP>
```

---

## 🚀 Unity Project

### ▶️ Run the Unity Project
1. Open the Unity project.
2. Play the scene.
3. Ensure TCP/IP settings in Unity match the ROS TCP server.
