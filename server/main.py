# MIT Reality Hack 2020 project #
# Weston Bell-Geddes, Jay Hesslink, Thomas Suarez #
# Jan 17-19, 2020 #

import socket
import threading
import asyncio
import websockets
import numpy

HOST = "0.0.0.0"
PORT = 8001

curWs = None
rotY = 0

def initWs():
    start_server = websockets.serve(handleWs, HOST, PORT)
    asyncio.get_event_loop().run_until_complete(start_server)
    asyncio.get_event_loop().run_forever()

async def sendWs(data):
    if curWs != None:
        await curWs.send(data)
        print("SENT via WebSocket: " + data)

async def parse(data):
    global rotY
    # await sendWs(data)

    print("RECV: " + str(data))
    rotY += 5
    if (rotY > 360):
        rotY -= 360

    transformStr = "0," + str(rotY) + ",0"
    print("SEND: " + transformStr)
    await sendWs(transformStr)

    # TODO add PoseCNN forwarding here

async def handleWs(websocket, path):
    global curWs
    curWs = websocket

    while True:
        data = await websocket.recv()
        parse(data)

def initUdp():
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.bind((HOST, PORT))

    while True:
        data, addr = sock.recvfrom(65535)
        asyncio.run(parse(data))

threadUdp = threading.Thread(target=initUdp)
threadUdp.start()
initWs()