#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = '04ae7602-15b6-4e8e-9744-18efd0aca730'

### GetUserById
apiUrl = "api/OrganizationUser/org/#{orgId}/action/GetUserById/#{id}"
result = make_request(:get, apiUrl, nil)
puts(result)
