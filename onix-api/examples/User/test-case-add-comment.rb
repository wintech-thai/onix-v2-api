#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

###
caseId = "your-case-uuid-here"
apiUrl = "api/CaseManagement/org/#{orgId}/action/AddComment/#{caseId}"
param = {
  Content: "{\"type\":\"doc\",\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"ต้องการข้อมูลเพิ่มเติม กรุณาช่วยชี้แจง\"}]}]}",
}

result = make_request(:post, apiUrl, param)
puts(result.to_json)
