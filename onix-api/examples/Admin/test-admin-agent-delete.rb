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
agentId = "006455ba-beb2-4b90-8471-3d1178dc547c" 
apiUrl = "admin-api/AdminAgent/org/global/action/DeleteAgentById/#{agentId}"
param = nil

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:delete, apiUrl, param)
puts(result)
