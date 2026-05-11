#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = 'global'
keyFile = ".token"

apiUrl = "admin-api/AdminCustomRole/org/#{orgId}/action/GetInitialUserRolePermissions"
param =  nil

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

result = make_request(:get, apiUrl, param)

json = result.to_json
puts(json)
