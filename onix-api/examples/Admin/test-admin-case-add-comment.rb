#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

keyFile = ".token"

###
caseId = "your-case-uuid-here"
apiUrl = "admin-api/AdminCaseManagement/org/global/action/AddComment/#{caseId}"
param = {
  Content: "{\"type\":\"doc\",\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"ตรวจสอบแล้ว กำลังดำเนินการแก้ไข\"}]}]}",
}

token = File.read(keyFile)

ENV['API_KEY'] = nil
ENV['ACCESS_TOKEN'] = token

result = make_request(:post, apiUrl, param)
puts(result.to_json)
