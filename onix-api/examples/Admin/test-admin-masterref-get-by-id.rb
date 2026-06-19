#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
keyFile = ".token"

###
masterRefId = "198b743a-4579-41ee-853f-a748f6a40825" # เปลี่ยนเป็น Master Ref ID ที่ต้องการทดสอบ
apiUrl = "admin-api/AdminMasterRef/org/global/action/GetMasterRefById/#{masterRefId}"

param = nil

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:get, apiUrl, param)
puts(result.to_json)
