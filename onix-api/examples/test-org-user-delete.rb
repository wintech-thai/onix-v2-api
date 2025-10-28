#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']
id = 'fe0a0bbb-d22e-4235-a4ff-1cb8a960f63e'

param = nil

### UpdateUserById
apiUrl = "api/OrganizationUser/org/#{orgId}/action/DeleteUserById/#{id}"
result = make_request(:delete, apiUrl, param)

puts(result)
