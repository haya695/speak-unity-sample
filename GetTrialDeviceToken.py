# -*- coding: utf-8 -*-

# Copyright (c) 2019, NTT DOCOMO, INC.
# All rights reserved.
#
# Redistribution and use in source and binary forms, with or without
# modification, are permitted provided that the following conditions are met:
#  Redistributions of source code must retain the above copyright notice,
#   this list of conditions and the following disclaimer.
#  Redistributions in binary form must reproduce the above copyright notice,
#   this list of conditions and the following disclaimer in the documentation
#   and/or other materials provided with the distribution.
#  Neither the name of the NTT DOCOMO, INC. nor the names of its contributors
#   may be used to endorse or promote products derived from this software
#   without specific prior written permission.
#
# THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
# ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
# WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
# DISCLAIMED. IN NO EVENT SHALL NTT DOCOMO, INC. BE LIABLE FOR ANY
# DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
# (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
# LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
# ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
# (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
# SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

import sys
import json
import platform
gmajor, gminor, grevision = platform.python_version_tuple()
if gmajor == '2':
    import urllib2
else:
    import urllib.request
import os
import subprocess

TARGET = "trial"
HOST="https://api-agentcraft.sebastien.ai"
CLIENT_SECRET="xxxxxxxx-xxxx-xxxx-xxxxxxxxxxxx"

def get_response_json(url, param=None, method="POST"):
    headers = {"Content-Type" : "application/json"}
    if param is not None:
        json_data = json.dumps(param).encode("utf-8")
        request = urllib.request.Request(url, data=json_data, headers=headers, method=method)
    else:
        request = urllib.request.Request(url, headers=headers)
    response = urllib.request.urlopen(request).read()
    print(json.dumps(json.loads(response.decode()), indent=2))
    return response


def fileoutput(filename, output):
    filename = "./." + TARGET + "_" + filename
    f = open(filename.encode(), "wb")
    f.write(output.encode())
    f.close()
    print("SAVE " + filename + " : " + output)


def fileexist(filename):
    return os.path.exists("./." + TARGET + "_" + filename)


def fileread(filename):
    f = open("./." + TARGET + "_" + filename, "r")
    output = f.read()
    print("READ " + filename + " : " + output)
    f.close()
    return output


if __name__ == '__main__':
    args = sys.argv
    if 1 < len(args):
        TARGET = args[1]
    device_id = ""
    if not fileexist("device_id"):   
        try:
            device_id_json = get_response_json(HOST + "/devices", {"client_secret" : CLIENT_SECRET})
            device_id = json.loads(device_id_json.decode())["device_id"]
        except urllib.error.HTTPError as e:
            print("デバイスIDの取得に失敗しました。")
            print(json.dumps({"status": e.code, "reason": e.reason}, indent=4))
            exit()
        fileoutput("device_id", device_id)
    else:
        device_id = fileread("device_id")

    if not fileexist("device_token"):
        try: 
            device_token_json = get_response_json(HOST + "/devices/token", {"device_id" : device_id})
            device_token = json.loads(device_token_json.decode())["device_token"]
        except urllib.error.HTTPError as e:
            print("デバイストークンの取得に失敗しました。")
            print(json.dumps({"status": e.code, "reason": e.reason}, indent=4))
            exit()
        fileoutput("device_token", device_token)
        refresh_token = json.loads(device_token_json.decode())["refresh_token"]
        fileoutput("refresh_token", refresh_token)
    else:
        device_token = fileread("device_token")
        refresh_token = fileread("refresh_token")

        # DeviceToken validation
        try: 
            validate_result = get_response_json(HOST + "/devices/token/status?device_token=" + device_token)
        except urllib.error.HTTPError as e:
            print("デバイストークンの検証に失敗しました。")
            print(json.dumps({"status": e.code, "reason": e.reason}, indent=4)) 
            exit()
        status = json.loads(validate_result.decode())["status"]
        if status != "valid":
            # Update DeviceToken by RefreshToken
            device_token_json = get_response_json(HOST + "/devices/token/refresh", {"refresh_token" : refresh_token})
            device_token = json.loads(device_token_json.decode())["device_token"]
            if device_token == "" or device_token is None:
                os.remove("./." + TARGET + "_device_token")
                os.remove("./." + TARGET + "_refresh_token")
                print("デバイストークンの更新に失敗しました。")
                print(json.dumps(device_token_json, indent=4))
            else:
                fileoutput("device_token", device_token)
                refresh_token = json.loads(device_token_json.decode())["refresh_token"]
                fileoutput("refresh_token", refresh_token)

