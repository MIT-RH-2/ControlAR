# MIT Reality Hack 2020 project #
# Weston Bell-Geddes, Jay Hesslink, Thomas Suarez #
# Jan 17-19, 2020 #

import socket
import threading
import asyncio
import websockets

HOST = "127.0.0.1"
PORT = 8001

curWs = None

def initWs():
    start_server = websockets.serve(handleWs, HOST, PORT)
    asyncio.get_event_loop().run_until_complete(start_server)
    asyncio.get_event_loop().run_forever()

async def sendWs(data):
    if curWs != None:
        await curWs.send(data)
        print("SENT via WebSocket: " + data)

async def parse(data):
    await sendWs(data)
    # TODO add PoseCNN forwarding here

async def handleWs(websocket, path):
    global curWs
    curWs = websocket

    while True:
        data = await websocket.recv()
        print(data)
        await websocket.send(data)

def initUdp():
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.bind((HOST, PORT))

    while True:
        data, addr = sock.recvfrom(1024)
        data = str(data) # convert bin -> str
        print(data)
        asyncio.run(parse(data))

threadUdp = threading.Thread(target=initUdp)
threadUdp.start()
initWs()