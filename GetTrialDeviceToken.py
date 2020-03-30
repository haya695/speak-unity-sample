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
CONFIG = {
    "trial":{
        "device_id":"XXXXXXXXXXXXXXXXXXXX",
        "uds":"https://users-v2.sebastien.ai"
    }
}

def get_response(url):
    response = None
    if gmajor == '2':
        response = urllib2.urlopen(url)
    else:
        response = urllib.request.urlopen(url)
    html = response.read()
    print(html.decode())
    return html


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
    if TARGET in CONFIG:
        device_id = CONFIG[TARGET]["device_id"]
        uds = CONFIG[TARGET]["uds"]
    else:
        print(TARGET + " is illegal argument.")
        exit()
    
    if device_id == "" or device_id is None:
        print("Illegal Device_ID.. please make sure argument is set properly.")
        exit()
    else:
        print("Device ID :" + device_id)
        print("Please register above ID as your device on User Dashboard. " + uds)
        print("下記リンク（↓）を使ってブラウザ等でデバイスIDを自分のアカウントに登録して下さい。")
        print(uds + "/dashboard/device_registration?confirm=yes&device_id=" + device_id)
        print("")
        if gmajor == '2':
            i = raw_input('Press any key AFTER registration >>> ')
        else:
            i = input('Press any key AFTER registration >>> ')

    if not fileexist("device_token"):
        device_token_json = get_response(uds + "/api/req_device_token?device_id=" + device_id)
        device_token = json.loads(device_token_json.decode())["device_token"]
        if device_token == "" or device_token is None:
            print("Failed to get Device Token. Check User Dashboard to make sure the Device ID is registered properly.")
            print("If the Device ID has been registered, please remove and register the Device ID again on Agentcraft.")
            print("Device Tokenの取得に失敗しました。Device IDがUser Dashboardで正しく登録されているのか確認して下さい。")
            print("もし登録されている場合は、一度登録済みIDをAgentcraftから削除してから再度登録して下さい。")
            print("Device ID: " + device_id)
            exit()
        else:
            fileoutput("device_token", device_token)
            refresh_token = json.loads(device_token_json.decode())["refresh_token"]
            fileoutput("refresh_token", refresh_token)
    else:
        device_token = fileread("device_token")
        refresh_token = fileread("refresh_token")

        # DeviceToken validation
        validate_result = get_response(uds + "/api/validate_device_token?device_token=" + device_token)
        status = json.loads(validate_result.decode())["status"]
        if status != "valid":
            # Update DeviceToken by RefreshToken
            device_token_json = get_response(uds + "/api/update_device_token?refresh_token=" + refresh_token)
            device_token = json.loads(device_token_json.decode())["device_token"]
            if device_token == "" or device_token is None:
                os.remove("./." + TARGET + "_device_token")
                os.remove("./." + TARGET + "_refresh_token")
                print("Failed to update Device Token by Refresh Token. Check User Dashboard to make sure the Device ID is registered properly.")
                print("If the Device ID has been registered, please remove and register the Device ID again on Agentcraft.")
                print("Device Tokenの更新に失敗しました。Device IDがUser Dashboardで正しく登録されているのか確認して下さい。")
                print("もし登録されている場合は、一度登録済みIDをAgentcraftから削除してから再度登録して下さい。")
                print("Device ID: " + device_id)
            else:
                fileoutput("device_token", device_token)
                refresh_token = json.loads(device_token_json.decode())["refresh_token"]
                fileoutput("refresh_token", refresh_token)

