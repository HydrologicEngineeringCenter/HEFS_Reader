# HEFS_Reader
A project to support reading HEFS data from the CNRFC
# HEFS_Reader
A project to support reading HEFS data from the CNRFC

https://www.cnrfc.noaa.gov/csv/


TimeSeriesOfEnsembleLocations class holds a list of IWatershedForecast

Each IWatershedForecast holds a list of IEnsemble

Each IEnsemble is a list of EnsembleMember  (at a Location)

an EnsembleMember is a time series.


| TimeSeries Of EnsembleLocations |                                              |                            |                  |             |             |   |             |             | 
|-------------------------------|----------------------------------------------|----------------------------|------------------|-------------|-------------|---|-------------|-------------| 
|                               | WatershedForecast (IssueDate 11/1/2013) |                            |                  |             |             |   |             |             | 
|                               |                                              | ensemble1 (at Location 1) |                  |             |             |   |             |             | 
|                               |                                              |                            | GMT              | member1     | member2     | … | membern-1   | membern     | 
|                               |                                              |                            | 11/1/2013 12:00  | 0.003001747 | 0.003001747 | … | 0.003001747 | 0.003001747 | 
|                               |                                              |                            | 11/1/2013 13:00  | 0.003001747 | 0.003001747 | … | 0.003001747 | 0.003001747 | 
|                               |                                              |                            | 11/1/2013 14:00  | 0.002966432 | 0.002966432 | … | 0.002966432 | 0.002966432 | 
|                               |                                              |                            | 11/1/2013 15:00  | 0.002966432 | 0.002966432 | … | 0.002966432 | 0.002966432 | 
|                               |                                              |                            | …                | …           | …           | … | …           | …           | 
|                               |                                              |                            | 11/15/2013 12:00 | 0.003496152 | 0.001942307 | … | 0.031430054 | 0.04650942  | 
|                               |                                              |                            |                  |             |             |   |             |             | 
|                               |                                              | ensemble2 (at Location 2) |                  |             |             |   |             |             | 
|                               |                                              |                            | GMT              | member1     | member2     | … | membern-1   | membern     | 
|                               |                                              |                            | 11/1/2013 12:00  | 2.1         | 0.005650347 | … | 0.005650347 | 0.01695104  | 
|                               |                                              |                            | 11/1/2013 13:00  | 2.1         | 0.005473774 | … | 0.005473774 | 0.016421321 | 
|                               |                                              |                            | 11/1/2013 14:00  | 2.1         | 0.005261886 | … | 0.005261886 | 0.015785657 | 
|                               |                                              |                            | 11/1/2013 15:00  | 2.1         | 0.005085312 | … | 0.005085312 | 0.015255936 | 
|                               |                                              |                            | …                | …           | …           | … | …           | …           | 
|                               |                                              |                            | 11/15/2013 12:00 | 6.24        | 0.00476748  | … | 0.0006      | 0.0018      | 
|                               |                                              |                            |                  |             |             |   |             |             | 
|                               | WatershedForecast (IssueDate 11/2/2013) |                            |                  |             |             |   |             |             | 
|                               |                                              | ensemble1 (at Location 1) |                  |             |             |   |             |             | 
|                               |                                              |                            | GMT              | member1     | member2     | … | membern-1   | membern     | 
|                               |                                              |                            | 41579.5          | 0.003001747 | 0.003001747 | … | 0.003001747 | 0.003001747 | 
|                               |                                              |                            | 41579.54167      | 0.003001747 | 0.003001747 | … | 0.003001747 | 0.003001747 | 
|                               |                                              |                            | …                |             |             |   |             |             | 
			
