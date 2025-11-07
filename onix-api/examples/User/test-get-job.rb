#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = "napbiotec"
id = "306b33bd-a99c-455a-b1f0-7a7cba8559ed"

### GetJobById
apiUrl = "api/Job/org/#{orgId}/action/GetJobById/#{id}"

result = make_request(:get, apiUrl, nil)
puts(result)
