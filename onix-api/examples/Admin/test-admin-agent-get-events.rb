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
agentId = "cdf71ee4-fb16-48dd-987e-6b6db52fb34f" 
apiUrl = "admin-api/AdminAgent/org/global/action/GetAgentEvents/#{agentId}"
apiUrl2 = "admin-api/AdminAgent/org/global/action/GetAgentEventCount/#{agentId}"
param = {
  FullTextSearch: "",
  EventType: "",
  Channel: "",
}

token = File.read(keyFile)

ENV['API_KEY'] = nil # ถ้าไม่ใช้ API KEY ก็เซ็ตเป็น nil
ENV['ACCESS_TOKEN'] = token

#puts("===[#{token}]")

result = make_request(:post, apiUrl, param)
puts(result.to_json)

result = make_request(:post, apiUrl2, param)
puts(result)
