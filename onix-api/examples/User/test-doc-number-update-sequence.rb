#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx'  # replace with actual id

# UpdateDocumentNumberSequence — ใช้ reset หรือ manual set ค่า sequence
apiUrl = "api/DocumentNumber/org/#{orgId}/action/UpdateDocumentNumberSequence/#{id}"
param = {
  CurrentSequenceNo: 0,
  CurrentSequenceKey: "",
}

result = make_request(:post, apiUrl, param)
json = result.to_json
puts(json)
