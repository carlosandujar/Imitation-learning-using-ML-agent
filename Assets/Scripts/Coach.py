import socket
import pandas as pd
pd.set_option('display.max_columns', None)
import numpy as np
from scipy.spatial import KDTree
from io import BytesIO
import threading
import queue
import time

def load_data():
    global top_position_data
    global bottom_position_data
    global top_shot_data
    global bottom_shot_data
    global top_position_kd
    global bottom_position_kd 
    global top_shot_kd
    global bottom_shot_kd
    # Specify the path to your Excel file
    excel_file_path = 'data.xlsx'

    # Read the Excel file into a Pandas DataFrame
    data = pd.read_excel(excel_file_path)
    columns_to_drop = ['Unnamed: 0', 'frame', 'time', 'role', 'role.1', 'role.2', 'role.3']
    data = data.drop(columns=columns_to_drop)

    # Filter data by column
    # Top position data -> 'last hit' == 'T'
    top_position_data = data[data['last hit'] == 'T']
    # Bot position data -> 'last hit' == 'B'
    bottom_position_data = data[data['last hit'] == 'B']
    # Top shot data -> shots from 'T'
    top_shot_data = top_position_data[top_position_data['shot'] != 'undef']
    # Top shot data -> shots from 'B'
    bottom_shot_data = bottom_position_data[bottom_position_data['shot'] != 'undef']

    # Transform data to numpy arrays
    top_position_data = top_position_data.values
    bottom_position_data = bottom_position_data.values
    top_shot_data = top_shot_data.values
    bottom_shot_data = bottom_shot_data.values

    # Create the corresponding KD-Trees
    top_position_kd = KDTree(top_position_data[:, 0:10])
    bottom_position_kd = KDTree(bottom_position_data[:, 0:10])
    top_shot_kd = KDTree(top_shot_data[:,0:10])
    bottom_shot_kd = KDTree(bottom_shot_data[:,0:10])

def process_shot_request(command_array):
    player_id = command_array[1]
    query_array = np.array([float(value.replace(',', '.')) for value in command_array[2:]]).astype(float)
    if (player_id == 'T1_1' or player_id == 'T1_2'):
        dist, index = bottom_shot_kd.query(query_array)
        return bottom_shot_data[index, 18:21]
    else:
        dist, index = top_shot_kd.query(query_array)
        return top_shot_data[index, 18:21]

def process_movement_request(command_array):
    player_id = command_array[1]
    lastHitByTeam = command_array[-1]
    query_array = np.array([float(value.replace(',', '.')) for value in command_array[2:-1]]).astype(float)
    if (lastHitByTeam == 'T1'):
        dist, index = top_position_kd.query(query_array)
        return np.concatenate((top_position_data[index, 0:8], top_position_data[index, 10:18]), axis=None)
    else:
        dist, index = bottom_position_kd.query(query_array)
        return np.concatenate((bottom_position_data[index, 0:8], bottom_position_data[index, 10:18]), axis=None)


def process_command(command):
    command_array = command.split()
    if (command_array[0] == 'SHOT_REQUEST'):
        start_time = time.time()
        nearest_sample = process_shot_request(command_array)
        end_time = time.time()
        elapsed_time = end_time - start_time
        response = 'SHOT_RESPONSE ' + command_array[1] + ' ' + ' '.join(str(value) for value in nearest_sample) + '\n'
        print("SENDING SHOT RESPONSE: " + response)
        print(f"Elapsed time: {elapsed_time} seconds")
        conn.send(response.encode('utf-8'))
    elif (command_array[0] == 'MOVEMENT_REQUEST'):
        start_time = time.time()
        nearest_sample = process_movement_request(command_array)
        end_time = time.time()
        elapsed_time = end_time - start_time
        response = 'MOVEMENT_RESPONSE ' + command_array[1] + ' ' + ' '.join(str(value) for value in nearest_sample) + '\n'
        print("SENDING MOVEMENT RESPONSE: " + response)
        print(f"Elapsed time: {elapsed_time} seconds")
        conn.send(response.encode('utf-8'))
    
def command_handler():
    print("HANDLING COMMANDS")
    while True:
        try:
            command = command_queue.get(timeout=1)  # Check the queue for new commands
            process_command(command)
        except queue.Empty:
            pass


def command_collector():
    print("COLLECTING COMMANDS")
    global conn
    global addr
    # Create a socket
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind(('127.0.0.1', 5555))
    server_socket.listen(1)

    print("Python script: Waiting for connections...")
    while True:
        # Accept a connection from Unity
        conn, addr = server_socket.accept()
        print(f"Connected to Unity at {addr}")
        while True:
            # Receive data from Unity
            command = conn.recv(1024).decode('utf-8')
            if not command:
                break
            commands = command.split("\n")
            for command in commands:
                if command:
                    command_queue.put(command)
            
        conn.close()

if __name__ == "__main__":
    global command_queue
    
    command_queue = queue.Queue()
    load_data()

    # Start the thread for collecting commands
    collect_thread = threading.Thread(target=command_collector)
    collect_thread.start()

    # Start the thread for handling commands
    handle_thread = threading.Thread(target=command_handler)
    handle_thread.start()



