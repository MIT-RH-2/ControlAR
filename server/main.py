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

# code by Quasimondo.
# see https://gist.github.com/Quasimondo/c3590226c924a06b276d606f4f189639
#input is an YUV numpy array with shape (height,width,3) can be uint,int, float or double,  values expected in the range 0..255
#output is a double RGB numpy array with shape (height,width,3), values in the range 0..255
def YUV2RGB( yuv ):
      
    m = np.array([[ 1.0, 1.0, 1.0],
                [-0.000007154783816076815, -0.3441331386566162, 1.7720025777816772],
                [ 1.4019975662231445, -0.7141380310058594 , 0.00001542569043522235] ])
    
    rgb = np.dot(yuv,m)
    rgb[:,:,0]-=179.45477266423404
    rgb[:,:,1]+=135.45870971679688
    rgb[:,:,2]-=226.8183044444304
    return rgb

threadUdp = threading.Thread(target=initUdp)
threadUdp.start()
initWs()