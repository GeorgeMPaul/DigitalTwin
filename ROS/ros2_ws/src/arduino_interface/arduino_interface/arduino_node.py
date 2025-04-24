import rclpy
from rclpy.node import Node
from std_msgs.msg import String
import serial
import json

class ArduinoInterface(Node):
    def __init__(self):
        super().__init__('arduino_interface')
        self.publisher = self.create_publisher(String, 'arduino_data', 10)
        self.serial_port = serial.Serial('/dev/ttyACM0', 115200, timeout=1.0)
        self.timer = self.create_timer(0.1, self.read_serial)
        self.get_logger().info('Arduino interface started')

    def read_serial(self):
        if self.serial_port.in_waiting > 0:
            try:
                line = self.serial_port.readline().decode('utf-8').strip()
                msg = String()
                msg.data = line
                self.publisher.publish(msg)
                self.get_logger().info(f'Published: {line}')
            except Exception as e:
                self.get_logger().error(f'Error reading serial: {str(e)}')

def main(args=None):
    rclpy.init(args=args)
    node = ArduinoInterface()
    rclpy.spin(node)
    node.destroy_node()
    rclpy.shutdown()

if __name__ == '__main__':
    main()