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

TARGET = "prod"
CONFIG = {
    "prod":{
        "client_secret":"XXXXXXXXXXXXXXXXXXXX",
        "devices":"https://doubk.aiplat.jp/v1.0/dvo/doubk/devices",
        "regist":"https://doufr.aiplat.jp/device/regist?directAccess=true&deviceId=",
        "token":"https://doubk.aiplat.jp/v1.0/dvo/doubk/devices/token",
        "refresh":"https://doubk.aiplat.jp/v1.0/dvo/doubk/devices/token/refresh"
    }
}

def post(config_key, param):
    url = CONFIG[TARGET][config_key]
    json_data = json.dumps(param).encode("utf-8")
    headers = {"Content-Type" : "application/json"}
    if gmajor == '2':
        request = urllib2.Request(url, data=json_data, headers=headers)
        response = urllib2.urlopen(request).read()
    else:
        request = urllib.request.Request(url, data=json_data, headers=headers)
        response = urllib.request.urlopen(request).read().decode()
    return json.loads(response)


def exists(filename):
    return os.path.exists("./." + TARGET + "_" + filename)

def write(filename, value):
    filename = "./." + TARGET + "_" + filename
    f = open(filename.encode(), "wb")
    f.write(value.encode())

def read(filename):
    f = open("./." + TARGET + "_" + filename, "r")
    value = f.read()
    f.close()
    return value

def validate(token):
    if "device_token" in token and "device_refresh_token" in token:
        return token["device_token"] != "None"
    return False

if __name__ == '__main__':
    args = sys.argv
    if 1 < len(args):
        if args[1] in CONFIG:
            TARGET = args[1]
        else:
            print("Please specify one of the following values as argument.")
            print("以下のいずれかの値を引数に指定して下さい。")
            for key in CONFIG:
                print(" " + key)
            exit()

    if not exists("device_id"):
        devices = post("devices",{"client_secret_device":CONFIG[TARGET]["client_secret"]})
        if "device_id" in devices:
            device_id = devices["device_id"]
            write("device_id", device_id)
            print("Success to get DeviceID :" + device_id)
            print("デバイスIDの取得に成功しました。")
            print("Please register DeviceID as your device on User Dashboard.")
            print("下記リンク（↓）を使ってブラウザ等でデバイスIDを自分のアカウントに登録して下さい。")
            print(CONFIG[TARGET]["regist"] + device_id)
            print("")
            if gmajor == '2':
                i = raw_input('Press any key AFTER registration >>> ')
            else:
                i = input('Press any key AFTER registration >>> ')
        else:
            print("Failed to get DeviceID.. please try again.")
            exit()

    else:
        device_id = read("device_id")


    if not exists("device_token"):
        verb_en = "get"
        verb_ja = "取得"
        token = post("token",{"device_id":device_id})
    else:
        verb_en = "update"
        verb_ja = "更新"
        token = post("refresh",{"device_refresh_token":read("refresh_token")})

    if validate(token):
        device_token = token["device_token"]
        refresh_token = token["device_refresh_token"]
        write("device_token", device_token)
        write("refresh_token", refresh_token)
        print("Success to {0} DeviceToken : {1}".format(verb_en,device_token))
        print("デバイストークンの{}に成功しました。".format(verb_ja))
        print("Success to {0} RefreshToken : {1}".format(verb_en,refresh_token))
        print("リフレッシュトークンの{}に成功しました。".format(verb_ja))
    else:
        os.remove("./." + TARGET + "_device_id")
        os.remove("./." + TARGET + "_device_token")
        os.remove("./." + TARGET + "_refresh_token")
        print("Failed to {} DeviceToken. Check User Dashboard to make sure the Device ID is registered properly.".format(verb_en))
        print("If the DeviceID has been registered, please remove and register the Device ID again on User Dashboard.")
        print("DeviceTokenの{}に失敗しました。DeviceIDがUser Dashboardで正しく登録されているのか確認して下さい。".format(verb_ja))
        print("もし登録されている場合は、一度登録済みIDを削除してから再度登録して下さい。")
        print("DeviceID: " + device_id)
